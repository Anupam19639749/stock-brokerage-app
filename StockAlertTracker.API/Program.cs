using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using StockAlertTracker.API.Data;
using StockAlertTracker.API.Helpers;
using StockAlertTracker.API.Hubs;
using StockAlertTracker.API.Interfaces.Repositories;
using StockAlertTracker.API.Interfaces.Services;
using StockAlertTracker.API.Repositories;
using StockAlertTracker.API.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// --- 1. Register DbContext ---
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// --- 2. Register Repositories and UnitOfWork ---
// Scoped: A new instance is created for each web request
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IWalletRepository, WalletRepository>();
builder.Services.AddScoped<IWalletTransactionRepository, WalletTransactionRepository>();
builder.Services.AddScoped<IPortfolioHoldingRepository, PortfolioHoldingRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IPriceAlertRepository, PriceAlertRepository>();
builder.Services.AddScoped<IPlatformStatsRepository, PlatformStatsRepository>();
builder.Services.AddScoped<IPasswordResetTokenRepository, PasswordResetTokenRepository>();

// --- 3. Register Services ---

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IPasswordService, PasswordService>();
builder.Services.AddScoped<IEmailService, EmailService>();

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IWalletService, WalletService>();
builder.Services.AddScoped<IAdminService, AdminService>();

builder.Services.AddScoped<IStockDataService, StockDataService>();
builder.Services.AddScoped<ITradeService, TradeService>();
builder.Services.AddScoped<IPortfolioService, PortfolioService>();

builder.Services.AddScoped<IAlertService, AlertService>();

// ---  Register Background Workers ---
builder.Services.AddHostedService<RealTimePriceWorker>();
builder.Services.AddHostedService<AnalyticsWorker>();

// ---  Add AutoMapper ---
builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddProfile<AutoMapperProfile>();
});

// ---  Add HttpContextAccessor (needed for services to access user ID) ---
builder.Services.AddHttpContextAccessor();

// --- SignalR Service ---
builder.Services.AddSignalR();

// --- Configure HttpClientFactory for Finnhub ---
builder.Services.AddHttpClient("Finnhub", client =>
{
    client.BaseAddress = new Uri("https://finnhub.io/api/v1/");
});

builder.Services.AddControllers();

// --- . Configure JWT Authentication ---
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"]!))
        };
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                // Try to read the token from the "token" cookie
                // This is for your React app
                if (context.Request.Cookies.ContainsKey("token"))
                {
                    context.Token = context.Request.Cookies["token"];
                }

                // If the token is not in the cookie, the middleware will 
                // automatically look for the "Authorization: Bearer" header
                // which is what Swagger uses.
                return Task.CompletedTask;
            }
        };
    });

// --- . Configure Swagger to use JWT ---
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "StockAlertTracker.API", Version = "v1" });

    // Add JWT "Authorize" button to Swagger UI
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: \"Bearer 12345abcdef\""
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
            new string[] {}
        }
    });
});

// --- . Register Background Workers ---
builder.Services.AddHostedService<RealTimePriceWorker>();

// --- Build the App ---
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// --- Add CORS (IMPORTANT for React) ---
// This allows your React app (e.g., from localhost:4200) to connect
app.UseCors(policy => policy
    .WithOrigins("http://localhost:5173" , "https://localhost:5173") // Your React app's address
    .AllowAnyHeader()
    .AllowAnyMethod()
    .AllowCredentials()); // Required for SignalR

// --- 8. Add Authentication and Authorization to the Pipeline ---
app.UseAuthentication(); // 1. Are you who you say you are?
app.UseAuthorization();  // 2. Do you have permission to be here?

app.MapControllers();

// --- NEW: Map the SignalR Hub Endpoint ---
app.MapHub<PriceHub>("/pricehub");

app.Run();