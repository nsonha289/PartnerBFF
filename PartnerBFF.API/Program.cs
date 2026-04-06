using Microsoft.AspNetCore.Authentication;
using Microsoft.OpenApi.Models;
using PartnerBFF.API.Authentications;
using PartnerBFF.API.Middlewares;
using PartnerBFF.Application.Interfaces;
using PartnerBFF.Application.Messaging;
using PartnerBFF.Application.Services;
using PartnerBFF.Infrastructure.Configurations;
using PartnerBFF.Infrastructure.Interfaces;
using PartnerBFF.Infrastructure.Messaging.RabbitMQ;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<RabbitMqSettings>(
    builder.Configuration.GetSection("RabbitMQ"));

var apiKeySettings = builder.Configuration
    .GetSection("ApiKey")
    .Get<ApiKeySettings>() ?? throw new Exception("ApiKey config not found");

builder.Services.AddSingleton(apiKeySettings);

// Register authentication
builder.Services.AddAuthentication("ApiKey")
    .AddScheme<AuthenticationSchemeOptions, ApiKeyAuthenticationHandler>(
        "ApiKey", _ => { });

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddSingleton<IRabbitMqConnectionFactory, RabbitMqConnectionFactory>();
builder.Services.AddSingleton<IMessagePublisher, RabbitMqPublisher>();

builder.Services.AddSingleton<IMessagePublisherBroker, MessagePublisherBroker>();
builder.Services.AddHttpClient<IPartnerVerifierService, PartnerVerifierService>(client =>
{
    var settings = builder.Configuration
        .GetSection("PartnerVerification")
        .Get<PartnerVerificationSettings>() ?? throw new Exception("PartnerVerification config base URL is not found");

    client.BaseAddress = new Uri(settings.BaseUrl);
});
builder.Services.AddScoped<ITransactionService, TransactionService>();
builder.Services.AddScoped<IAuthorizationService, AuthorizationService>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    // 1. Define the API Key security scheme
    options.AddSecurityDefinition(AppConstants.API_KEY, new OpenApiSecurityScheme
    {
        Name = AppConstants.API_KEY_HEADER,         
        Type = SecuritySchemeType.ApiKey,
        In = ParameterLocation.Header,
        Description = "Enter your API key in the field below"
    });

    // 2. Apply it globally to all endpoints
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id   = AppConstants.API_KEY        
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseExceptionHandler();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
