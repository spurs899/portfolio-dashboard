using PortfolioManager.Core.Services;
using PortfolioManager.Core.Services.Brokerage;
using PortfolioManager.Core.Services.Market;
using Sentry.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddMemoryCache();
builder.Services.AddHttpClient();

// Market services
builder.Services.AddHttpClient<IMarketDataProvider, PolygonMarketDataProvider>();
builder.Services.AddScoped<IOfflineMarketStatusCalculator, NyseOfflineMarketStatusCalculator>();

// Brokerage clients
builder.Services.AddHttpClient<SharesiesClient>()
    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
    {
        UseCookies = true,
        CookieContainer = new System.Net.CookieContainer()
    });

builder.Services.AddScoped<ISharesiesClient>(sp => sp.GetRequiredService<SharesiesClient>());
builder.Services.AddScoped<ISharesiesAuthClient>(sp => sp.GetRequiredService<SharesiesClient>());
builder.Services.AddScoped<ISharesiesDataClient>(sp => sp.GetRequiredService<SharesiesClient>());

builder.Services.AddHttpClient<InteractiveBrokersClient>()
    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
    {
        UseCookies = true,
        CookieContainer = new System.Net.CookieContainer()
    });

builder.Services.AddScoped<IInteractiveBrokersClient>(sp => sp.GetRequiredService<InteractiveBrokersClient>());
builder.Services.AddScoped<IIbkrAuthClient>(sp => sp.GetRequiredService<InteractiveBrokersClient>());
builder.Services.AddScoped<IIbkrDataClient>(sp => sp.GetRequiredService<InteractiveBrokersClient>());

// Brokerage services - Register all implementations with ISP interfaces
builder.Services.AddScoped<IQrAuthenticationService, IbkrQrAuthenticationService>();
builder.Services.AddScoped<SharesiesBrokerageService>();
builder.Services.AddScoped<InteractiveBrokersBrokerageService>();
builder.Services.AddScoped<IBrokerageService, SharesiesBrokerageService>();
builder.Services.AddScoped<IBrokerageService, InteractiveBrokersBrokerageService>();
builder.Services.AddScoped<IBrokerageAuthenticationService, SharesiesBrokerageService>(sp => sp.GetRequiredService<SharesiesBrokerageService>());
builder.Services.AddScoped<IBrokeragePortfolioService, SharesiesBrokerageService>(sp => sp.GetRequiredService<SharesiesBrokerageService>());
builder.Services.AddScoped<IBrokerageAuthenticationService, InteractiveBrokersBrokerageService>(sp => sp.GetRequiredService<InteractiveBrokersBrokerageService>());
builder.Services.AddScoped<IBrokeragePortfolioService, InteractiveBrokersBrokerageService>(sp => sp.GetRequiredService<InteractiveBrokersBrokerageService>());
builder.Services.AddScoped<IBrokerageServiceFactory, BrokerageServiceFactory>();

// Legacy coordinator
builder.Services.AddScoped<PortfolioManager.Core.Coordinators.ISharesiesCoordinator, PortfolioManager.Core.Coordinators.SharesiesCoordinator>();

builder.WebHost.UseSentry((SentryAspNetCoreOptions  options) =>
{
    builder.Configuration.GetSection("Sentry").Bind(options);
    
    options.MinimumBreadcrumbLevel = LogLevel.Debug;
    options.MinimumEventLevel = LogLevel.Information;
    
    // Diagnostic settings
    options.Debug = builder.Environment.IsDevelopment();
    options.DiagnosticLevel = builder.Environment.IsDevelopment() ? SentryLevel.Debug : SentryLevel.Error;
    
    // Environment
    options.Environment = builder.Environment.EnvironmentName;
    options.EnableLogs = true;
    
    // Optional but useful
    options.TracesSampleRate = 0.0; // set >0 only if you want performance monitoring
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorWasm",
        policy => policy
            .WithOrigins(
                "http://localhost:5262", 
                "https://localhost:7262",
                "https://spurs899.github.io")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials());
});

var app = builder.Build();

// Use Sentry request tracking
app.UseSentryTracing();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.Use(async (context, next) =>
    {
        if (context.Request.Path == "/")
        {
            context.Response.Redirect("/swagger");
            return;
        }
        await next();
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowBlazorWasm");
app.UseRouting();
app.MapControllers();

SentrySdk.CaptureMessage("Sentry Initialised");

app.Run();