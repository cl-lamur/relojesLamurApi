using Microsoft.EntityFrameworkCore;
using RelojesLamur.API.Entities;

namespace RelojesLamur.API.Data;

public static class DatabaseSeeder
{
    // ?? Reset completo: borra tablas, aplica migración y hace seed ??
    public static async Task ResetAndSeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var logger  = scope.ServiceProvider.GetRequiredService<ILogger<AppDbContext>>();

        logger.LogWarning("RESET: Borrando tablas existentes...");
        await DropAllTablesAsync(context, logger);

        logger.LogInformation("RESET: Aplicando migración InitialCreate...");
        await context.Database.MigrateAsync();

        await SeedAdminAsync(context, logger);

        logger.LogInformation("RESET: Base de datos restaurada correctamente.");
    }

    // ?? Startup normal: aplica migraciones pendientes y hace seed ??
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var logger  = scope.ServiceProvider.GetRequiredService<ILogger<AppDbContext>>();

        await context.Database.MigrateAsync();
        logger.LogInformation("Migraciones aplicadas correctamente.");

        await SeedAdminAsync(context, logger);
    }

    // ?? Drop de tablas en orden (respeta FK constraints) ????????????
    private static async Task DropAllTablesAsync(AppDbContext ctx, ILogger logger)
    {
        var sql = """
            SET FOREIGN_KEY_CHECKS = 0;
            DROP TABLE IF EXISTS `OrderItems`;
            DROP TABLE IF EXISTS `Orders`;
            DROP TABLE IF EXISTS `ProductFeatures`;
            DROP TABLE IF EXISTS `Products`;
            DROP TABLE IF EXISTS `Users`;
            DROP TABLE IF EXISTS `__EFMigrationsHistory`;
            SET FOREIGN_KEY_CHECKS = 1;
            """;

        // Ejecutar cada sentencia por separado (Pomelo no admite multibatch)
        foreach (var stmt in sql.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (string.IsNullOrWhiteSpace(stmt)) continue;
            await ctx.Database.ExecuteSqlRawAsync(stmt);
        }

        logger.LogWarning("RESET: Todas las tablas eliminadas.");
    }

    private static async Task SeedAdminAsync(AppDbContext ctx, ILogger logger)
    {
        if (await ctx.Users.IgnoreQueryFilters().AnyAsync(u => u.Email == "admin@lamur.com"))
            return;

        ctx.Users.Add(new User
        {
            Name = "Administrador Lamur",
            Email = "admin@lamur.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!", 12),
            Role = "admin",
            CreatedAt = DateTime.UtcNow
        });
        await ctx.SaveChangesAsync();
        logger.LogInformation("Seed: usuario admin creado (admin@lamur.com).");
    }
}