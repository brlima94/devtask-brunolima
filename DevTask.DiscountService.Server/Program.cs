using DevTask.DiscountService.Server.Data;
using DevTask.DiscountService.Server.Services;
using DevTask.DiscountService.Server.Settings;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace DevTask.DiscountService.Server;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Logging.ClearProviders();
        builder.Logging.AddSimpleConsole(options =>
        {
            options.IncludeScopes = true;
            options.UseUtcTimestamp = true;
            options.TimestampFormat = "yyyy-MM-dd HH:mm:ss ";
        });
        
        var settings = builder.Configuration.GetRequiredSection("DiscountSettings").Get<DiscountSettings>()!;

        builder.Services.AddSingleton(settings);
        builder.Services.AddGrpc();
        builder.Services.AddDbContext<DiscountContext>(options =>
        {
            options.UseSqlServer(settings.ConnectionStrings.DiscountDb);
        });
        builder.Services.AddScoped<IDiscountCodeService, DiscountCodeService>();
        builder.Services.AddSingleton<IConnectionMultiplexer>(await ConnectionMultiplexer.ConnectAsync(settings.ConnectionStrings.Redis));

        var app = builder.Build();
        var logger = app.Services.GetRequiredService<ILogger<Program>>();

        logger.LogInformation("Applying DB migration(s)");
        using (var scope = app.Services.CreateScope())
        {
            await using var db = scope.ServiceProvider.GetRequiredService<DiscountContext>();
            await db.Database.MigrateAsync();
        }
        logger!.LogInformation("DB migration(s) completed");

        app.MapGrpcService<DiscountGrpcService>();
        app.MapGet("/",
            () =>
                "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

        await app.RunAsync();
    }
}