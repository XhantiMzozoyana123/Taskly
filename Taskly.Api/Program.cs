using Hangfire;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Taskly.Application.Interfaces;
using Taskly.Application.Services;
using Taskly.Domain;
using Taskly.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// -----------------------------------------------------
// 1️⃣ Configuration
// -----------------------------------------------------
builder.Configuration
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

// -----------------------------------------------------
// 2️⃣ Logging
// -----------------------------------------------------
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// -----------------------------------------------------
// 3️⃣ Database + Hangfire
// -----------------------------------------------------
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHangfire(config => config
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHangfireServer();

// -----------------------------------------------------
// 4️⃣ Application Services
// -----------------------------------------------------
builder.Services.AddScoped<IAiService, AiService>();
builder.Services.AddScoped<ILLMService, LLMService>();
builder.Services.AddScoped<IExtractService, ExtractService>();
builder.Services.AddScoped<ISenderService, SenderService>();
builder.Services.AddScoped<ICookieService, CookieService>();
builder.Services.AddScoped<ICampaignService, CampaignService>();
builder.Services.AddScoped<IFacebookService, FacebookService>();
builder.Services.AddScoped<IInstagramService, InstagramService>();
builder.Services.AddScoped<ITwitterService, TwitterService>();
builder.Services.AddScoped<IRedditService, RedditService>();
builder.Services.AddScoped<ITikTokService, TikTokService>();
builder.Services.AddScoped<IShortcutService, ShortcutService>();

builder.Services.AddHttpClient();
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddSingleton<IUiLogger, UiLogger>();

builder.Services.AddControllers();

// -----------------------------------------------------
// 5️⃣ Build + Run
// -----------------------------------------------------
var app = builder.Build();

app.UseHttpsRedirection();
app.UseAuthorization();

app.UseHangfireDashboard("/hangfire");

app.UseStaticFiles(); // ✅ Enable serving static files from wwwroot

app.MapControllers();
// Default root endpoint
app.MapGet("/", () => Results.Content("<h1>Taskly API is running ✅</h1>", "text/html"));

app.Run();
