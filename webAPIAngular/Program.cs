using System.Text;
using System.Threading.RateLimiting;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using RelojesLamur.API.Common;
using RelojesLamur.API.Data;
using RelojesLamur.API.Mappings;
using RelojesLamur.API.Middleware;
using RelojesLamur.API.Services;
using RelojesLamur.API.Services.Interfaces;
using Serilog;

// ?? Serilog ??????????????????????????????????????????????????
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File("logs/relojeslamur-.log", rollingInterval: RollingInterval.Day)
    .Enrich.FromLogContext()
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();

// ?? MySQL (Pomelo) ???????????????????????????????????????????
var connStr = builder.Configuration.GetConnectionString("DefaultConnection")!;
builder.Services.AddDbContext<AppDbContext>(options =>
options.UseMySql(connStr, ServerVersion.AutoDetect(connStr),
    mySqlOptions => mySqlOptions.EnableRetryOnFailure(
        maxRetryCount: 5,
        maxRetryDelay: TimeSpan.FromSeconds(10),
        errorNumbersToAdd: null)));

// ?? JWT ??????????????????????????????????????????????????????
var jwt = builder.Configuration.GetSection("Jwt");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidateAudience         = true,
            ValidateLifetime         = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer              = jwt["Issuer"],
            ValidAudience            = jwt["Audience"],
            IssuerSigningKey         = new SymmetricSecurityKey(
                                            Encoding.UTF8.GetBytes(jwt["Secret"]!))
        };
    });

// ?? CORS ?????????????????????????????????????????????????????
var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? [];

builder.Services.AddCors(o =>
    o.AddPolicy("LamurCors", p =>
        p.WithOrigins(allowedOrigins)
         .AllowAnyHeader()
         .AllowAnyMethod()));

// ?? Rate Limiting (login: 10 req/min por IP) ?????????????????
builder.Services.AddRateLimiter(options =>
{
    options.AddPolicy<string>("LoginPolicy", ctx =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            }));
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});




// ?? Controllers + respuesta de validación estandarizada ??????
builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(o =>
    {
        o.InvalidModelStateResponseFactory = ctx =>
        {
            var errors = ctx.ModelState
                .Where(e => e.Value?.Errors.Count > 0)
                .SelectMany(e => e.Value!.Errors.Select(er =>
                {
                    var campo = e.Key.Length > 0 ? $"[{e.Key}]" : "[body]";
                    var valorEnviado = !string.IsNullOrEmpty(e.Value!.AttemptedValue)
                        ? $" — valor recibido: \"{e.Value.AttemptedValue}\""
                        : string.Empty;
                    var mensaje = !string.IsNullOrWhiteSpace(er.ErrorMessage)
                        ? er.ErrorMessage
                        : er.Exception is not null
                            ? $"Formato incorrecto: {er.Exception.Message}"
                            : "Valor inválido.";
                    return $"{campo}: {mensaje}{valorEnviado}";
                }))
                .ToList();
            return new BadRequestObjectResult(
                ApiResponse.Fail("Validación fallida. Revisa los campos indicados.", errors));
        };
    });







// ?? FluentValidation ?????????????????????????????????????????
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// ?? AutoMapper ???????????????????????????????????????????????


builder.Services.AddAutoMapper(typeof(MappingProfile));

// ?? Servicios de negocio ?????????????????????????????????????
builder.Services.AddScoped<IJwtService,     JwtService>();
builder.Services.AddScoped<IAuthService,    AuthService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IOrderService,   OrderService>();
builder.Services.AddScoped<IUserService,    UserService>();
builder.Services.AddScoped<FirebaseStorageService>();
builder.Services.AddHttpContextAccessor();

// ?? Swagger con boton Authorize JWT ??????????????????????????
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title       = "Relojes Lamur API",
        Version     = "v1",
        Description = "API REST para el e-commerce de relojes de lujo Relojes Lamur"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name         = "Authorization",
        Type         = SecuritySchemeType.ApiKey,
        Scheme       = "Bearer",
        BearerFormat = "JWT",
        In           = ParameterLocation.Header,
        Description  = "Formato: Bearer {token}"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id   = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

    // Necesario para que Swashbuckle pueda documentar endpoints con IFormFile
    c.MapType<IFormFile>(() => new OpenApiSchema { Type = "string", Format = "binary" });
});


// ?????????????????????????????????????????????????????????????
var app = builder.Build();

// ?? Reset mode: dotnet run -- --reset ????????????????????????
if (args.Contains("--reset"))
{
    app.Logger.LogWarning("Modo --reset activado. Restaurando base de datos...");
    await DatabaseSeeder.ResetAndSeedAsync(app.Services);
    app.Logger.LogInformation("Reset completado. La API NO se inicia en modo reset.");
    return;
}

// ?? Seed de base de datos ????????????????????????????????????
await DatabaseSeeder.SeedAsync(app.Services);

// ?? Pipeline ?????????????????????????????????????????????????
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Relojes Lamur API v1");
    c.RoutePrefix = string.Empty;
});

app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseStaticFiles();
app.UseCors("LamurCors");
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));
app.Run();
