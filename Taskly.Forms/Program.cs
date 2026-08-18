using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Windows.Forms;
using Taskly.Application.Interfaces;
using Taskly.Domain;
using Taskly.Forms.Forms;
using Taskly.Infrastructure.Services;

namespace Taskly.Forms
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            // 🧱 Local SQLite database (localhost)
            const string connectionString =
                "Data Source=taskly.db";

            // ✅ Build the host with hardcoded configuration
            var host = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    // ------------------------ DbContext ------------------------
                    services.AddDbContext<ApplicationDbContext>(options =>
                        options.UseSqlite(connectionString));

                    // ------------------------ HTTP + Infrastructure ------------------------
                    services.AddHttpClient();
                    services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
                    services.AddSingleton<IUiLogger, UiLogger>();

                    // ------------------------ Application Services ------------------------
                    services.AddScoped<IAiService, AiService>();
                    services.AddScoped<ILLMService, LLMService>();
                    services.AddScoped<IExtractService, ExtractService>();
                    services.AddScoped<ISenderService, SenderService>();
                    services.AddScoped<ICookieService, CookieService>();
                    services.AddScoped<ICampaignService, CampaignService>();
                    services.AddScoped<IFacebookService, FacebookService>();
                    services.AddScoped<IInstagramService, InstagramService>();
                    services.AddScoped<ITwitterService, TwitterService>();
                    services.AddScoped<IRedditService, RedditService>();
                    services.AddScoped<ITikTokService, TikTokService>();
                    services.AddScoped<IAirbnbService, AirbnbService>();

                    // ------------------------ UI (Forms) ------------------------
                    services.AddTransient<Forms.Taskly>();
                })
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddConsole();
                })
                .Build();

            // ✅ Auto-create the local SQLite database schema at startup
            using (var scope = host.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                dbContext.Database.EnsureCreated();
            }

            ApplicationConfiguration.Initialize();

            // Start the main form (DI-enabled)
            var mainForm = host.Services.GetRequiredService<Forms.Taskly>();
            System.Windows.Forms.Application.Run(mainForm);
        }
    }
}
