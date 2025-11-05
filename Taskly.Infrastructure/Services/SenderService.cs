using Microsoft.EntityFrameworkCore;
using Microsoft.Playwright;
using Serilog.Events; // This namespace might not be needed if not using Serilog directly here
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Taskly.Application.Dtos;
using Taskly.Application.Interfaces;
using Taskly.Domain;
using Taskly.Domain.Entities;

namespace Taskly.Infrastructure.Services
{
    public class SenderService : ISenderService
    {
        private readonly ApplicationDbContext _context;
        private readonly IFacebookService _facebookService;
        private readonly IInstagramService _instagramService;
        private readonly ITwitterService _twitterService;
        private readonly ITikTokService _tikTokService;
        private readonly IAiService _aiService;
        private readonly ICookieService _cookieService;
        private readonly IUiLogger _logger; // Injecting IUiLogger

        private readonly Random _random = new Random();

        public SenderService(
            ApplicationDbContext context,
            IFacebookService facebookService,
            IInstagramService instagramService,
            ITwitterService twitterService,
            ITikTokService tikTokService,
            IAiService aiService,
            ICookieService cookieService,
            IUiLogger logger) // Add IUiLogger to the constructor
        {
            _context = context;
            _facebookService = facebookService;
            _instagramService = instagramService;
            _twitterService = twitterService;
            _tikTokService = tikTokService;
            _aiService = aiService;
            _cookieService = cookieService;
            _logger = logger; // Initialize the logger
        }

        public async Task StartCampaignMessages(MessengerDto messengerDto, List<Leads> leads)
        {
            if (string.IsNullOrWhiteSpace(messengerDto.Text) && !messengerDto.TextList.Any())
            {
                _logger.LogWarning("Message text is empty and no message list provided. Skipping messaging sequence.");
                return;
            }

            IBrowser browser = null;
            IPage page = null;

            try
            {
                _logger.LogInfo("Starting messaging sequence.");

                // 🔹 Load cookie accounts
                var cookieAccounts = await _context.CookieFiles
                    .OrderBy(x => x.Id)
                    .ToListAsync();

                if (!cookieAccounts.Any())
                {
                    _logger.LogError("No active cookie accounts found for rotation. Cannot proceed with messaging.");
                    throw new Exception("No active cookie accounts found for rotation.");
                }
                _logger.LogInfo($"Found {cookieAccounts.Count} cookie accounts for rotation.");


                leads.GroupBy(g => g.Name)
                     .ToList();

                if (!leads.Any())
                {
                    _logger.LogInfo("No new leads found to message. Exiting messaging sequence.");
                    return;
                }
                _logger.LogInfo($"Found {leads.Count} new leads to process.");


                int accountIndex = 0;
                var random = new Random();

                for (int i = 0; i < leads.Count; i++)
                {
                    var lead = leads[i];
                    _logger.LogInfo($"Processing lead {i + 1}/{leads.Count}: {lead.Name} ({lead.Platform})");

                    // 🔁 Account Rotation
                    CookieFiles currentAccount;
                    if (messengerDto.AccountRotation)
                    {
                        currentAccount = cookieAccounts[accountIndex];
                        accountIndex = (accountIndex + 1) % cookieAccounts.Count;
                        _logger.LogInfo($"Using account '{currentAccount.FileName}' for lead '{lead.Name}' (Rotation enabled).");
                    }
                    else
                    {
                        // Use single account
                        currentAccount = cookieAccounts.First();
                        _logger.LogInfo($"Using account '{currentAccount.FileName}' for lead '{lead.Name}' (No rotation).");
                    }

                    (page, browser) = await _cookieService.LoadCookieOnPageAsync(currentAccount.FileName, messengerDto.PrivateMode);
                    _logger.LogInfo($"Loaded cookie file '{currentAccount.FileName}' on browser page for lead '{lead.Name}'.");

                    // 🧠 Message Rotation
                    string messageText;
                    if (messengerDto.MessegeRotation && messengerDto.TextList.Any())
                    {
                        messageText = messengerDto.TextList[random.Next(messengerDto.TextList.Count)];
                        _logger.LogInfo("Using a rotated message from the provided list.");
                    }
                    else
                    {
                        messageText = messengerDto.Text;
                        _logger.LogInfo("Using a single, static message.");
                    }

                    // Replace placeholders dynamically
                    var iceBreaker = await _context.Icebreakers.FirstOrDefaultAsync(x => x.LeadId == lead.Id);
                    string iceBreakerText = iceBreaker?.Text ?? "Hey!";
                    if (iceBreaker == null)
                    {
                        _logger.LogWarning($"No icebreaker found for LeadId: {lead.Id}. Using default 'Hey!'.");
                    }

                    // Example of custom field replacement
                    var customMessage = await _context.CustomMessages.FirstOrDefaultAsync(x => x.LeadId == lead.Id);
                    string customMessageText = iceBreaker?.Text ?? "Hey!";
                    if (iceBreaker == null)
                    {
                        _logger.LogWarning($"No custom message found for LeadId: {lead.Id}. Using default 'Hey!'.");
                    }

                    messageText = messageText
                        .Replace("[name]", lead.Name ?? "there") // Provide a fallback for name
                        .Replace("[descr]", lead.PostDescription ?? "their recent post") // Provide fallback
                        .Replace("[url]", lead.PostUrl ?? "") // Provide fallback
                        .Replace("[date]", lead.PostDate.ToString("yyyy-MM-dd")) // Format date
                        .Replace("[icebreak]", iceBreakerText)
                        .Replace("[custom]", customMessageText); // Example of custom field replacement
                    _logger.LogInfo($"Constructed message for '{lead.Name}' (first 100 chars): {messageText.Substring(0, Math.Min(messageText.Length, 100))}...");


                    // Build new message DTO
                    var message = new MessengerDto
                    {
                        Text = messageText,
                        Lead = lead,
                        MessageDelay = messengerDto.MessageDelay // Propagate delay
                    };

                    bool sent = await SendDM(message, page);

                    if (sent)
                    {
                        _logger.LogInfo($"Successfully sent DM to '{lead.Name}' on {lead.Platform}.");
                        await UpdateLead(lead); // Update lead status after successful send
                        _logger.LogInfo($"Lead '{lead.Name}' status updated to 'Contacted'.");
                    }
                    else
                    {
                        _logger.LogWarning($"Failed to send DM to '{lead.Name}' on {lead.Platform}.");
                    }

                    // Always close the browser page for the current account after attempting to send a DM
                    if (browser != null)
                    {
                        await browser.CloseAsync();
                        _logger.LogInfo($"Browser closed for account '{currentAccount.FileName}'.");
                    }


                    // Simulate human delay
                    int humanDelay = random.Next(3, 6); // 3-6 seconds between processing leads
                    _logger.LogInfo($"Simulating human delay for {humanDelay} seconds before next lead.");
                    await Task.Delay(TimeSpan.FromSeconds(humanDelay));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"An unhandled error occurred during MessagingSequenceAsync. Exception: {ex.Message}");
                throw;
            }
            finally
            {
                // Ensure browser is closed even if an exception occurs mid-loop
                if (browser != null)
                {
                    await browser.CloseAsync();
                    _logger.LogInfo("Browser closed in finally block of MessagingSequenceAsync.");
                }
            }
        }

        public async Task StartMessages(MessengerDto messengerDto)
        {
            if (string.IsNullOrWhiteSpace(messengerDto.Text) && !messengerDto.TextList.Any())
            {
                _logger.LogWarning("Message text is empty and no message list provided. Skipping messaging sequence.");
                return;
            }

            IBrowser browser = null;
            IPage page = null;

            try
            {
                _logger.LogInfo("Starting messaging sequence.");

                // 🔹 Load cookie accounts
                var cookieAccounts = await _context.CookieFiles
                    .OrderBy(x => x.Id)
                    .ToListAsync();

                if (!cookieAccounts.Any())
                {
                    _logger.LogError("No active cookie accounts found for rotation. Cannot proceed with messaging.");
                    throw new Exception("No active cookie accounts found for rotation.");
                }
                _logger.LogInfo($"Found {cookieAccounts.Count} cookie accounts for rotation.");

                // 🔹 Load leads
                var leads = await _context.Leads
                    .Where(x => x.Status == "New")
                    .ToListAsync();

                leads.GroupBy(g => g.Name)
                     .ToList();

                if (!leads.Any())
                {
                    _logger.LogInfo("No new leads found to message. Exiting messaging sequence.");
                    return;
                }
                _logger.LogInfo($"Found {leads.Count} new leads to process.");


                int accountIndex = 0;
                var random = new Random();

                for (int i = 0; i < leads.Count; i++)
                {
                    var lead = leads[i];
                    _logger.LogInfo($"Processing lead {i + 1}/{leads.Count}: {lead.Name} ({lead.Platform})");

                    // 🔁 Account Rotation
                    CookieFiles currentAccount;
                    if (messengerDto.AccountRotation)
                    {
                        currentAccount = cookieAccounts[accountIndex];
                        accountIndex = (accountIndex + 1) % cookieAccounts.Count;
                        _logger.LogInfo($"Using account '{currentAccount.FileName}' for lead '{lead.Name}' (Rotation enabled).");
                    }
                    else
                    {
                        // Use single account
                        currentAccount = cookieAccounts.First();
                        _logger.LogInfo($"Using account '{currentAccount.FileName}' for lead '{lead.Name}' (No rotation).");
                    }

                    (page, browser) = await _cookieService.LoadCookieOnPageAsync(currentAccount.FileName, messengerDto.PrivateMode);
                    _logger.LogInfo($"Loaded cookie file '{currentAccount.FileName}' on browser page for lead '{lead.Name}'.");

                    // 🧠 Message Rotation
                    string messageText;
                    if (messengerDto.MessegeRotation && messengerDto.TextList.Any())
                    {
                        messageText = messengerDto.TextList[random.Next(messengerDto.TextList.Count)];
                        _logger.LogInfo("Using a rotated message from the provided list.");
                    }
                    else
                    {
                        messageText = messengerDto.Text;
                        _logger.LogInfo("Using a single, static message.");
                    }

                    // Replace placeholders dynamically
                    var iceBreaker = await _context.Icebreakers.FirstOrDefaultAsync(x => x.LeadId == lead.Id);
                    string iceBreakerText = iceBreaker?.Text ?? "Hey!";
                    if (iceBreaker == null)
                    {
                        _logger.LogWarning($"No icebreaker found for LeadId: {lead.Id}. Using default 'Hey!'.");
                    }

                    // Example of custom field replacement
                    var customMessage = await _context.CustomMessages.FirstOrDefaultAsync(x => x.LeadId == lead.Id);
                    string customMessageText = iceBreaker?.Text ?? "Hey!";
                    if (iceBreaker == null)
                    {
                        _logger.LogWarning($"No custom message found for LeadId: {lead.Id}. Using default 'Hey!'.");
                    }

                    messageText = messageText
                        .Replace("[name]", lead.Name ?? "there") // Provide a fallback for name
                        .Replace("[descr]", lead.PostDescription ?? "their recent post") // Provide fallback
                        .Replace("[url]", lead.PostUrl ?? "") // Provide fallback
                        .Replace("[date]", lead.PostDate.ToString("yyyy-MM-dd")) // Format date
                        .Replace("[icebreak]", iceBreakerText)
                        .Replace("[custom]", customMessageText); // Example of custom field replacement
                    _logger.LogInfo($"Constructed message for '{lead.Name}' (first 100 chars): {messageText.Substring(0, Math.Min(messageText.Length, 100))}...");


                    // Build new message DTO
                    var message = new MessengerDto
                    {
                        Text = messageText,
                        Lead = lead,
                        MessageDelay = messengerDto.MessageDelay // Propagate delay
                    };

                    bool sent = await SendDM(message, page);

                    if (sent)
                    {
                        _logger.LogInfo($"Successfully sent DM to '{lead.Name}' on {lead.Platform}.");
                        await UpdateLead(lead); // Update lead status after successful send
                        _logger.LogInfo($"Lead '{lead.Name}' status updated to 'Contacted'.");
                    }
                    else
                    {
                        _logger.LogWarning($"Failed to send DM to '{lead.Name}' on {lead.Platform}.");
                    }

                    // Always close the browser page for the current account after attempting to send a DM
                    if (browser != null)
                    {
                        await browser.CloseAsync();
                        _logger.LogInfo($"Browser closed for account '{currentAccount.FileName}'.");
                    }


                    // Simulate human delay
                    int humanDelay = random.Next(3, 6); // 3-6 seconds between processing leads
                    _logger.LogInfo($"Simulating human delay for {humanDelay} seconds before next lead.");
                    await Task.Delay(TimeSpan.FromSeconds(humanDelay));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"An unhandled error occurred during MessagingSequenceAsync. Exception: {ex.Message}");
                throw;
            }
            finally
            {
                // Ensure browser is closed even if an exception occurs mid-loop
                if (browser != null)
                {
                    await browser.CloseAsync();
                    _logger.LogInfo("Browser closed in finally block of MessagingSequenceAsync.");
                }
            }
        }

        private async Task<bool> SendDM(MessengerDto messengerDto, IPage page)
        {
            try
            {
                // Random delay between messages
                int preMessageDelay = _random.Next(15_000, 45_000); // 15–45 seconds
                _logger.LogInfo($"Applying pre-message random delay of {preMessageDelay / 1000} seconds before sending DM to '{messengerDto.Lead.Name}'.");
                await Task.Delay(preMessageDelay);

                if (messengerDto.Lead.Status == "New")
                {
                    _logger.LogInfo($"Attempting to send DM to '{messengerDto.Lead.Name}' on platform: {messengerDto.Lead.Platform}.");
                    switch (messengerDto.Lead.Platform.ToLower())
                    {
                        case "facebook":
                            await _facebookService.DirectMessagingAsync(page, messengerDto);
                            break;
                        case "instagram":
                            await _instagramService.DirectMessagingAsync(page, messengerDto);
                            break;
                        case "twitter":
                            await _twitterService.DirectMessagingAsync(page, messengerDto);
                            break;
                        case "tiktok":
                            await _tikTokService.DirectMessagingAsync(page, messengerDto);
                            break;
                        default:
                            string errorMessage = $"Unsupported platform '{messengerDto.Lead.Platform}' specified for lead '{messengerDto.Lead.Name}'.";
                            _logger.LogError(errorMessage);
                            throw new ArgumentException(errorMessage);
                    }

                    // The lead status update and post-message delay should happen *after* successful sending
                    // which is now handled in the main loop for clarity and to ensure a browser close.
                    await UpdateLead(messengerDto.Lead); // This will be done in the main loop
                    await Task.Delay(messengerDto.MessageDelay); // This delay is handled in the main loop for the next lead

                    _logger.LogInfo($"DM sent successfully to '{messengerDto.Lead.Name}' on {messengerDto.Lead.Platform}.");
                    return true;
                }
                else
                {
                    _logger.LogInfo($"Lead '{messengerDto.Lead.Name}' status is not 'New' ({messengerDto.Lead.Status}), skipping DM.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error sending DM to '{messengerDto.Lead.Name}' on {messengerDto.Lead.Platform}. Exception: {ex.Message}");
            }

            return false;
        }

        private async Task UpdateLead(Leads lead)
        {
            var query = await _context.Leads.Where(x => x.Name == lead.Name).ToListAsync();

            foreach (var item in query)
            {
                item.Status = "Contacted";
                _context.Leads.Update(item);
                await _context.SaveChangesAsync();
                _logger.LogInfo($"Lead '{item.Name}' status updated to 'Contacted' in the database.");
            }
        }
    }
}