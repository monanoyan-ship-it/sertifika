using System.Text;
using Sertifika.Context;
using Sertifika.DependencyInjection;
using Sertifika.Middleware;
using Sertifika.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// DI - EntityServices + Factories + Infrastructure
builder.Services.AddApplicationServices();

// JWT Service
builder.Services.AddScoped<IJwtService, JwtService>();

// Email Service
builder.Services.AddScoped<IEmailService, EmailService>();

// OneDrive Services
builder.Services.AddSingleton<OneDriveOAuthService>();
builder.Services.AddScoped<IOneDriveService, OneDriveService>();

// PDF Service (Python)
builder.Services.AddHttpClient<IPdfService, PdfService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["PdfService:BaseUrl"]!);
    client.Timeout = TimeSpan.FromMinutes(5);
});

// Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"]!;
var jwtIssuer = builder.Configuration["Jwt:Issuer"]!;
var jwtAudience = builder.Configuration["Jwt:Audience"]!;

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
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        RoleClaimType = System.Security.Claims.ClaimTypes.Role
    };
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            if (string.IsNullOrEmpty(context.Token))
            {
                var cookieToken = context.Request.Cookies["auth_token"];
                if (!string.IsNullOrEmpty(cookieToken))
                {
                    context.Token = cookieToken;
                    return Task.CompletedTask;
                }

                var queryToken = context.Request.Query["access_token"];
                if (!string.IsNullOrEmpty(queryToken))
                {
                    context.Token = queryToken;
                }
            }
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();

// CSRF / Antiforgery
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-XSRF-TOKEN";
    options.Cookie.Name = "XSRF-TOKEN-SERVER";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.SuppressXFrameOptionsHeader = true;
});

// Controllers + MVC Views
builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS
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

// Auto-migration on startup
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.Migrate();
}

// Uploads klasorlerini olustur
var uploadsRoot = Path.Combine(app.Environment.WebRootPath ?? Path.Combine(app.Environment.ContentRootPath, "wwwroot"), "uploads");
foreach (var folder in new[] { "signatures", "backgrounds", "certificates" })
{
    var folderPath = Path.Combine(uploadsRoot, folder);
    if (!Directory.Exists(folderPath))
        Directory.CreateDirectory(folderPath);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseExceptionHandler("/Panel/Error/500");
}

app.UseStatusCodePagesWithReExecute("/Panel/Error/{0}");

app.UseHttpsRedirection();
app.UseSecurityHeaders();
app.UseCors("AllowAll");
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgeryTokens();
app.MapControllers();
app.MapControllerRoute(name: "default", pattern: "{controller=Panel}/{action=Login}/{id?}");

app.Run();
