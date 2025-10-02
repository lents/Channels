var builder = WebApplication.CreateBuilder(args);

using RealWorld.WebApi.Services;

// Add services to the container.

// Register the ChannelService as a singleton to ensure a single instance is used throughout the app.
builder.Services.AddSingleton<ChannelService>();

// Register the BackgroundJobService as a hosted service.
// This will start the service when the application starts and stop it gracefully on shutdown.
builder.Services.AddHostedService<BackgroundJobService>();

builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();