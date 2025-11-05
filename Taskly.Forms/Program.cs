using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
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
            var host = Host.CreateDefaultBuilder()
                .ConfigureAppConfiguration((context, config) =>
                {
                    // ✅ Ensure correct path
                    config.SetBasePath(AppContext.BaseDirectory);
                    config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                })
                .ConfigureServices((context, services) =>
                {
                    var configuration = context.Configuration;

                    // ✅ Register HttpClient support
                    services.AddHttpClient();

                    // ------------------------ DbContext ------------------------
                    services.AddDbContext<ApplicationDbContext>(options =>
                        options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

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

                    services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
                    services.AddSingleton<IUiLogger, UiLogger>();

                    // ------------------------ Forms (UI) ------------------------
                    services.AddTransient<Forms.Taskly>();
                })
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddConsole();
                })
                .Build();

            ApplicationConfiguration.Initialize();

            var mainForm = host.Services.GetRequiredService<Forms.Taskly>();
            System.Windows.Forms.Application.Run(mainForm);
        }
    }
}
