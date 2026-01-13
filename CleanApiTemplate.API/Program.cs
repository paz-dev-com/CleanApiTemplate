using CleanApiTemplate.API.Middleware;
using CleanApiTemplate.API.Services;
using CleanApiTemplate.Application;
using CleanApiTemplate.Core.Interfaces;
using CleanApiTemplate.Data;
using CleanApiTemplate.Data.Seeders;
using CleanApiTemplate.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog for structured logging
// Note: Serilog internally handles culture-specific formatting
#pragma warning disable CA1305 // Serilog handles IFormatProvider internally
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/api-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();
#pragma warning restore CA1305

builder.Host.UseSerilog();

// Add services to the container.

// Register layers using extension methods
builder.Services.AddApplication();
builder.Services.AddInfrastructure();
builder.Services.AddPersistence(builder.Configuration);

// Register API services
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

// Add HttpContextAccessor for CurrentUserService
builder.Services.AddHttpContextAccessor();

// Configure JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey is not configured");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ClockSkew = TimeSpan.Zero // Remove default 5 minute tolerance
    };

    // Configure for HTTPS only in production
    options.RequireHttpsMetadata = builder.Environment.IsProduction();
});

builder.Services.AddAuthorization();

builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Clean API Template",
        Version = "v1",
        Description = "A Clean Architecture API template with .NET 10 demonstrating modern patterns, security, and best practices",
    });

    // Add JWT authentication to Swagger
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Add CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Automatically apply migrations and create database on startup
using (var scope = app.Services.CreateScope())
{
    try
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<CleanApiTemplate.Data.Persistence.ApplicationDbContext>();
        Log.Information("Checking database...");

        // Create database if it doesn't exist and apply any pending migrations
        await dbContext.Database.MigrateAsync();

        Log.Information("Database ready");

        // Seed default data (roles with random GUIDs per environment)
        var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
        await seeder.SeedAsync();

        Log.Information("Database seeding completed");
    }
    catch (Exception ex)
    {
        Log.Error(ex, "An error occurred while migrating or seeding the database");
        throw;
    }
}

// Configure the HTTP request pipeline.

// Add global exception handling middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Clean API Template v1");
        c.RoutePrefix = "swagger";
    });
}

// Use Serilog request logging
app.UseSerilogRequestLogging();

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new
{
    status = "healthy",
    timestamp = DateTime.UtcNow,
    environment = app.Environment.EnvironmentName,
    version = "1.0.0"
}))
   .WithName("HealthCheck")
   .WithTags("Health")
   .AllowAnonymous();

// Database health check
app.MapGet("/health/db", async (IHealthCheckService healthCheckService) =>
{
    try
    {
        var isHealthy = await healthCheckService.CheckDatabaseHealthAsync();
        
        if (isHealthy)
        {
            return Results.Ok(new { status = "healthy", database = "connected" });
        }
        
        return Results.Problem(
            detail: "Database connection failed",
            statusCode: StatusCodes.Status503ServiceUnavailable);
    }
    catch (Exception ex)
    {
        return Results.Problem(
            detail: ex.Message,
            statusCode: StatusCodes.Status503ServiceUnavailable);
    }
})
   .WithName("DatabaseHealthCheck")
   .WithTags("Health")
   .AllowAnonymous();

try
{
    Log.Information("Starting Clean API Template application");
    Log.Information("Environment: {Environment}", app.Environment.EnvironmentName);
    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    await Log.CloseAndFlushAsync();
}
