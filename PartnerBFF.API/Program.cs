using PartnerBFF.API.Middlewares;
using PartnerBFF.Application.Interfaces;
using PartnerBFF.Application.Messaging;
using PartnerBFF.Application.Models.Requests;
using PartnerBFF.Application.Services;
using PartnerBFF.Infrastructure.Configurations;
using PartnerBFF.Infrastructure.Interfaces;
using PartnerBFF.Infrastructure.Messaging.RabbitMQ;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<RabbitMqSettings>(
    builder.Configuration.GetSection("RabbitMQ"));

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

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseExceptionHandler();

app.UseAuthorization();

app.MapControllers();

app.Run();
