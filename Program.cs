using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using backend.Data;
using backend.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register repositories
builder.Services.AddScoped<backend.Repositories.Interfaces.IDoctorRepository, backend.Repositories.DoctorRepository>();

// Register services
builder.Services.AddScoped<backend.Services.Interfaces.IDoctorService, backend.Services.DoctorService>();
builder.Services.AddScoped<backend.Services.ISubscriptionPlanService, backend.Services.SubscriptionPlanService>();
builder.Services.AddScoped<JwtService>(); // Add JWT service
builder.Services.AddSingleton<backend.Services.ILocalizationService, backend.Services.LocalizationService>(); // Add Localization service

// Configure Paddle settings
builder.Services.Configure<backend.Services.PaddleSettings>(
    builder.Configuration.GetSection("Paddle"));

// Configure Subscription settings
builder.Services.Configure<backend.Configuration.SubscriptionSettings>(
    builder.Configuration.GetSection("Subscription"));

// Register Paddle service with HttpClient
builder.Services.AddHttpClient<backend.Services.IPaddleService, backend.Services.PaddleService>();

// Configure JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not configured");
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? throw new InvalidOperationException("JWT Issuer not configured");
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? throw new InvalidOperationException("JWT Audience not configured");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ValidateIssuer = true,
        ValidIssuer = jwtIssuer,
        ValidateAudience = true,
        ValidAudience = jwtAudience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// Configure JSON serialization to use camelCase
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

// Configure form options for file uploads
builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 10 * 1024 * 1024; // 10MB limit
});

// Add Swagger/OpenAPI services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Salymed API",
        Version = "v1",
        Description = "Salymed Backend API with Paddle Integration and JWT Authentication"
    });

    // Add JWT Authentication to Swagger
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
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

// Add CORS - Updated to support ngrok and Paddle webhooks
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy => policy
            .SetIsOriginAllowed(origin =>
            {
                // Allow requests without origin (e.g., Paddle webhooks, Postman, curl)
                if (string.IsNullOrEmpty(origin))
                    return true;

                // Allow localhost on any port
                if (Uri.TryCreate(origin, UriKind.Absolute, out var uri))
                {
                    // Allow localhost and 127.0.0.1
                    if (uri.Host == "localhost" || uri.Host == "127.0.0.1")
                        return true;

                    // Allow ngrok domains
                    if (uri.Host.EndsWith(".ngrok-free.dev") || uri.Host.EndsWith(".ngrok.io"))
                        return true;

                    // Allow Paddle domains
                    if (uri.Host.EndsWith(".paddle.com"))
                        return true;
                }
                return false;
            })
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials());
});

var app = builder.Build();

// Apply PreferredLanguage migration if needed
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    try
    {
        // Check if PreferredLanguage column exists, if not add it
        context.Database.ExecuteSqlRaw(@"
            IF NOT EXISTS (
                SELECT * FROM sys.columns
                WHERE object_id = OBJECT_ID(N'[dbo].[Users]')
                AND name = 'PreferredLanguage'
            )
            BEGIN
                ALTER TABLE [dbo].[Users]
                ADD PreferredLanguage NVARCHAR(5) NOT NULL DEFAULT 'az';
            END
        ");
        Console.WriteLine("✅ PreferredLanguage column migration completed");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"⚠️ PreferredLanguage migration warning: {ex.Message}");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Salymed API v1");
        c.RoutePrefix = "swagger"; // Swagger UI will be at /swagger
    });
}

app.UseCors("AllowFrontend");

// Static files configuration for avatars
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(
        Path.Combine(Directory.GetCurrentDirectory(), "wwwroot")),
    RequestPath = ""
});

app.UseRouting();
app.UseAuthentication(); // Add authentication middleware
app.UseAuthorization();
app.MapControllers();

app.Run();
