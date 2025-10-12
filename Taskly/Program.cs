using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Taskly.Application.Constants;
using Taskly.Application.Dtos;
using Taskly.Application.Interfaces;
using Taskly.Domain;
using Taskly.Domain.Entities;
using Taskly.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// ------------------------ Configuration ------------------------
builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddEnvironmentVariables();

// ------------------------ Serilog ------------------------

// ------------------------ DbContext ------------------------
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ------------------------ Identity ------------------------
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// ------------------------ JWT Authentication ------------------------
var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrEmpty(jwtKey))
    throw new Exception("JWT key not found in configuration.");

var keyBytes = Encoding.UTF8.GetBytes(jwtKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = true;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
        ValidateIssuer = false,
        ValidateAudience = false,
        ClockSkew = TimeSpan.Zero
    };
});

// ------------------------ Authorization (Subscription Levels) ------------------------
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("BasicOrAbove", policy =>
        policy.RequireAssertion(context =>
            context.User.HasClaim(SubscriptionConstants.Type, SubscriptionConstants.Basic) ||
            context.User.HasClaim(SubscriptionConstants.Type, SubscriptionConstants.Pro) ||
            context.User.HasClaim(SubscriptionConstants.Type, SubscriptionConstants.Premium)
        ));

    options.AddPolicy("ProOrAbove", policy =>
        policy.RequireAssertion(context =>
            context.User.HasClaim(SubscriptionConstants.Type, SubscriptionConstants.Pro) ||
            context.User.HasClaim(SubscriptionConstants.Type, SubscriptionConstants.Premium)
        ));

    options.AddPolicy("PremiumOnly", policy =>
        policy.RequireClaim(SubscriptionConstants.Type, SubscriptionConstants.Premium));
});


// ------------------------ CORS ------------------------
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// ------------------------ Application Services ------------------------
builder.Services.AddHttpClient();
builder.Services.AddHttpContextAccessor();
builder.Services.AddMemoryCache();

builder.Services.AddScoped<IAiService, AiService>();
builder.Services.AddScoped<ILLMService, LLMService>();
builder.Services.AddScoped<IExtractService, ExtractService>();
builder.Services.AddScoped<ISenderService, SenderService>();
builder.Services.AddScoped<IFacebookService, FacebookService>();
builder.Services.AddScoped<IInstagramService, InstagramService>();
builder.Services.AddScoped<ITwitterService, TwitterService>();
builder.Services.AddScoped<IRedditService, RedditService>();    
builder.Services.AddScoped<ITikTokService, TikTokService>();

builder.Services.Configure<EmailSettingDto>(builder.Configuration.GetSection("EmailSettings"));


builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
