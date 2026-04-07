using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Mvc;
using Udemy.FakePayment.API.Settings;

var builder = WebApplication.CreateBuilder(args);

// Iyzipay Settings
builder.Services.Configure<IyzipaySettings>(
    builder.Configuration.GetSection("Iyzipay"));

// Add services to the container.
JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Remove("sub");
builder.Services.AddControllers(options =>
{
    options.Filters.Add(new AuthorizeFilter());
})
.ConfigureApiBehaviorOptions(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        foreach (var error in context.ModelState)
        {
            foreach (var inner in error.Value.Errors)
            {
                Console.WriteLine($"[FakePayment API Validation Error] Field: {error.Key}, Error: {inner.ErrorMessage}");
            }
        }
        return new BadRequestObjectResult(context.ModelState);
    };
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var rabbitMqHost = builder.Configuration["RabbitMQUrl"] ?? "localhost";
var rabbitMqPort = ushort.TryParse(builder.Configuration["RabbitMQPort"], out var parsedRabbitMqPort)
    ? parsedRabbitMqPort
    : (ushort)5672;

// Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = builder.Configuration["IdentityServerURL"];
        options.Audience = "resource_payment";
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false
        };
    });

// MassTransit + RabbitMQ (InvoiceRequested event publish için)
builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(rabbitMqHost, rabbitMqPort, "/", host =>
        {
            host.Username("guest");
            host.Password("guest");
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

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
