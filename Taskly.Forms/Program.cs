using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Taskly.Application.Interfaces;
using Taskly.Infrastructure.Services;
using Taskly.Domain;
using Taskly.Forms.Forms;

namespace Taskly.Forms
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Create a Host (similar to ASP.NET Core)
            var host = Host.CreateDefaultBuilder()
                .ConfigureAppConfiguration((context, config) =>
                {
                    config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                })
                .ConfigureServices((context, services) =>
                {
                    var configuration = context.Configuration;

                    // ------------------------ DbContext ------------------------
                    services.AddDbContext<ApplicationDbContext>(options =>
                        options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

                    // ------------------------ Application Services ------------------------
                    services.AddScoped<IAiService, AiService>();
                    services.AddScoped<ILLMService, LLMService>();
                    services.AddScoped<IExtractService, ExtractService>();
                    services.AddScoped<ISenderService, SenderService>();
                    services.AddScoped<IRedditService, RedditService>();
                    services.AddScoped<IInstagramService, InstagramService>();
                    services.AddScoped<ITwitterService, TwitterService>();

                    // ------------------------ Forms (UI) ------------------------
                    services.AddTransient<Forms.Taskly>();
                })
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddConsole();
                })
                .Build();

            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            System.Windows.Forms.Application.Run(new Forms.Taskly());
        }
    }
}