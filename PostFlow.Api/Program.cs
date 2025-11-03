using Taskly.Application.Interfaces;
using Taskly.Domain.Entities;
using Taskly.Infrastructure.Services;
using Taskly.Domain;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Configure DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register services
builder.Services.AddScoped(typeof(IEntityService<>), typeof(EntityService<>));
builder.Services.AddScoped<IAiService, AiService>();
builder.Services.AddScoped<ILLMService, LLMService>(); // AiService depends on ILLMService

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowChromeExtension",
        builder =>
        {
            builder.WithOrigins("chrome-extension://<YOUR_EXTENSION_ID>") // Replace with actual extension ID
                   .AllowAnyHeader()
                   .AllowAnyMethod();
        });
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseCors("AllowChromeExtension"); // Use the CORS policy

app.MapControllers();

app.Run();
