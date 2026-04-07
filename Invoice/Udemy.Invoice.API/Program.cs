using MassTransit;
using Udemy.Invoice.API.Consumers;
using Udemy.Invoice.API.Services;
using Udemy.Invoice.API.Settings;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// EmailSettings Options Pattern
builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection("EmailSettings"));

// Email Service
builder.Services.AddScoped<IEmailService, EmailService>();

var rabbitMqHost = builder.Configuration["RabbitMQUrl"] ?? "localhost";
var rabbitMqPort = ushort.TryParse(builder.Configuration["RabbitMQPort"], out var parsedRabbitMqPort)
    ? parsedRabbitMqPort
    : (ushort)5672;

// MassTransit + RabbitMQ
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<InvoiceRequestedConsumer>();
    
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(rabbitMqHost, rabbitMqPort, "/", host =>
        {
            host.Username("guest");
            host.Password("guest");
        });
        
        cfg.ReceiveEndpoint("invoice-requested-service", e =>
        {
            e.ConfigureConsumer<InvoiceRequestedConsumer>(context);
        });
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

