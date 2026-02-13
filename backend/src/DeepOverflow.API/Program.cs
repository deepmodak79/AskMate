using DeepOverflow.Infrastructure.Persistence;
using DeepOverflow.Application;
using DeepOverflow.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using AspNetCoreRateLimit;
using DeepOverflow.Application.Common.Interfaces;
using DeepOverflow.API.Services;
using DeepOverflow.API.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/deepoverflow-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Swagger configuration
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Deep Overflow API",
        Version = "v1",
        Description = "Enterprise Stack Overflow for RMES",
        Contact = new OpenApiContact
        {
            Name = "RMES Engineering",
            Email = "deepoverflow-support@rmes.com"
        }
    });

    // JWT Authentication in Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Database
var defaultConnection = builder.Configuration.GetConnectionString("DefaultConnection") ?? string.Empty;
var useSqliteDevDb = string.IsNullOrWhiteSpace(defaultConnection)
    || defaultConnection.Contains("your_password_here", StringComparison.OrdinalIgnoreCase)
    || defaultConnection.Contains("your_password", StringComparison.OrdinalIgnoreCase);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    if (useSqliteDevDb)
    {
        // Simple local dev DB (no Postgres needed)
        options.UseSqlite($"Data Source=deepoverflow.dev.db");
    }
    else
    {
        options.UseNpgsql(
            defaultConnection,
            b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName));
    }
});

// JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JWT");
var secretKey = jwtSettings["Secret"] ?? throw new InvalidOperationException("JWT Secret not configured");

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
        ClockSkew = TimeSpan.Zero
    };

    // SignalR JWT authentication
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
            {
                context.Token = accessToken;
            }
            
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireModeratorRole", policy => policy.RequireRole("Moderator", "Admin"));
    options.AddPolicy("RequireAdminRole", policy => policy.RequireRole("Admin"));
});

// CORS
var corsSettings = builder.Configuration.GetSection("CORS");
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        var origins = corsSettings.GetSection("AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
        policy.WithOrigins(origins)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// Rate Limiting
builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
builder.Services.Configure<IpRateLimitPolicies>(builder.Configuration.GetSection("IpRateLimitPolicies"));
builder.Services.AddInMemoryRateLimiting();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

// SignalR
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = builder.Environment.IsDevelopment();
    options.KeepAliveInterval = TimeSpan.FromSeconds(15);
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
});

// Application & Infrastructure layers
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

// HTTP Context accessor
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

// Health checks
builder.Services.AddHealthChecks();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Deep Overflow API v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseSerilogRequestLogging();

// Keep HTTP working in dev (we're currently listening on http://localhost:5000)
// Enable HTTPS redirection only when you configure an HTTPS endpoint.

app.UseCors("AllowFrontend");

app.UseIpRateLimiting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

// SignalR hubs
app.MapHub<NotificationHub>("/hubs/notifications");

// Create dev database automatically when using SQLite
if (useSqliteDevDb)
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.EnsureCreated();
}

try
{
    Log.Information("Starting Deep Overflow API");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application start-up failed");
}
finally
{
    Log.CloseAndFlush();
}

public partial class Program { }
