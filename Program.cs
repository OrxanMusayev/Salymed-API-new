using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.StaticFiles;
using backend.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register repositories
builder.Services.AddScoped<backend.Repositories.Interfaces.IDoctorRepository, backend.Repositories.DoctorRepository>();

// Register services
builder.Services.AddScoped<backend.Services.Interfaces.IDoctorService, backend.Services.DoctorService>();
builder.Services.AddScoped<backend.Services.ISubscriptionPlanService, backend.Services.SubscriptionPlanService>();

// Configure Paddle settings
builder.Services.Configure<backend.Services.PaddleSettings>(
    builder.Configuration.GetSection("Paddle"));

// Register Paddle service with HttpClient
builder.Services.AddHttpClient<backend.Services.IPaddleService, backend.Services.PaddleService>();

// Configure JSON serialization to use camelCase
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });

// Configure form options for file uploads
builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 10 * 1024 * 1024; // 10MB limit
});
builder.Services.AddOpenApi();

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

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
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
app.UseAuthorization();
app.MapControllers();

app.Run();
