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
        await SeedProductsAsync(context, logger);

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
        await SeedProductsAsync(context, logger);
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

    private static async Task SeedProductsAsync(AppDbContext ctx, ILogger logger)
    {
        if (await ctx.Products.IgnoreQueryFilters().AnyAsync())
            return;

        var watches = new (Product p, ProductFeatures f)[]
        {
            // ?? Hombre ??????????????????????????????????????
            (new Product { Name = "Lamur Explorer I", Description = "Reloj de exploración con bisel negro anodizado y resistencia extrema para aventureros.", Price = 349.99m, Category = "hombre", ImageUrl = "/images/explorer-i.jpg" },
             new ProductFeatures { Material = "Acero inoxidable 316L", Mechanism = "Automático ETA 2824-2", WaterResistance = "300m" }),

            (new Product { Name = "Lamur Mariner Pro", Description = "Diseñado para buceadores profesionales con correa de caucho vulcanizado y cerámica.", Price = 449.99m, Category = "hombre", ImageUrl = "/images/mariner-pro.jpg" },
             new ProductFeatures { Material = "Cerámica y titanio grado 5", Mechanism = "Automático Sellita SW200", WaterResistance = "500m" }),

            (new Product { Name = "Lamur Racing Heritage", Description = "Inspirado en la alta velocidad. Cronógrafo con taquímetro y pulsómetro integrados.", Price = 289.99m, Category = "hombre", ImageUrl = "/images/racing-heritage.jpg" },
             new ProductFeatures { Material = "Acero inoxidable y fibra de carbono", Mechanism = "Cuarzo cronógrafo Swiss ISA 8171", WaterResistance = "100m" }),

            (new Product { Name = "Lamur Aviator GMT", Description = "Doble zona horaria para el viajero moderno. Bisel giratorio bidireccional.", Price = 499.99m, Category = "hombre", ImageUrl = "/images/aviator-gmt.jpg" },
             new ProductFeatures { Material = "Acero inoxidable PVD negro", Mechanism = "Automático NH34A GMT", WaterResistance = "200m" }),

            (new Product { Name = "Lamur Pilot Classic", Description = "Estética militar y precisión suiza. Esfera negro mate con marcadores Super-LumiNova.", Price = 429.99m, Category = "hombre", ImageUrl = "/images/pilot-classic.jpg" },
             new ProductFeatures { Material = "Acero inoxidable cepillado", Mechanism = "Automático ETA 2892-A2", WaterResistance = "100m" }),

            (new Product { Name = "Lamur Diver 300", Description = "Reloj de buceo sport con válvula de helio y cerrojo de seguridad en la correa.", Price = 249.99m, Category = "hombre", ImageUrl = "/images/diver-300.jpg" },
             new ProductFeatures { Material = "Acero inoxidable 904L", Mechanism = "Automático Miyota 9015", WaterResistance = "300m" }),

            // ?? Mujer ???????????????????????????????????????
            (new Product { Name = "Lamur Rose Datejust", Description = "Elegancia atemporal con esfera nacarada y bisel engastado de cristales.", Price = 549.99m, Category = "mujer", ImageUrl = "/images/rose-datejust.jpg" },
             new ProductFeatures { Material = "Oro rosa 18K y acero", Mechanism = "Automático Cal. 3235", WaterResistance = "100m" }),

            (new Product { Name = "Lamur Constellation Lady", Description = "Forma estrella característica en bisel, brazalete integrado y esfera de brillantes.", Price = 389.99m, Category = "mujer", ImageUrl = "/images/constellation-lady.jpg" },
             new ProductFeatures { Material = "Acero inoxidable y oro amarillo", Mechanism = "Cuarzo Swiss ETA F04.111", WaterResistance = "30m" }),

            (new Product { Name = "Lamur Diamond Tank", Description = "Silueta rectangular clásica con incrustaciones de cristales Swarovski en caja.", Price = 479.99m, Category = "mujer", ImageUrl = "/images/diamond-tank.jpg" },
             new ProductFeatures { Material = "Acero PVD dorado", Mechanism = "Cuarzo Swiss Ronda 762", WaterResistance = "30m" }),

            (new Product { Name = "Lamur Pearl Collection", Description = "Correa de piel italiana genuina y esfera con motivo floral sobre madreperla.", Price = 399.99m, Category = "mujer", ImageUrl = "/images/pearl-collection.jpg" },
             new ProductFeatures { Material = "Acero inoxidable y madreperla", Mechanism = "Cuarzo Miyota 1L45", WaterResistance = "50m" }),

            (new Product { Name = "Lamur Elegance Slim", Description = "Perfil ultra delgado de 5 mm. Minimalismo francés en su máxima expresión.", Price = 229.99m, Category = "mujer", ImageUrl = "/images/elegance-slim.jpg" },
             new ProductFeatures { Material = "Acero inoxidable cepillado", Mechanism = "Cuarzo Swiss ISA 1198/250", WaterResistance = "30m" }),

            (new Product { Name = "Lamur Petite Perle", Description = "Diseño compacto y femenino con corona de zafiro sintético. Ideal para uso diario.", Price = 129.99m, Category = "mujer", ImageUrl = "/images/petite-perle.jpg" },
             new ProductFeatures { Material = "Acero inoxidable 316L", Mechanism = "Cuarzo Ronda 784", WaterResistance = "50m" }),
        };

        foreach (var (product, features) in watches)
        {
            product.CreatedAt = DateTime.UtcNow;
            ctx.Products.Add(product);
            await ctx.SaveChangesAsync();

            features.ProductId = product.Id;
            ctx.ProductFeatures.Add(features);
        }

        await ctx.SaveChangesAsync();
        logger.LogInformation("Seed: 12 productos de relojes creados.");
    }
}

