using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Udemy.Order.Application.Consumers;
using Udemy.Order.Application.Handlers;
using Udemy.Order.Infrastructure;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Remove("sub");
builder.Services.AddControllers(options =>
{
    options.Filters.Add(new AuthorizeFilter());
}).ConfigureApiBehaviorOptions(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        foreach (var error in context.ModelState)
        {
            foreach (var inner in error.Value.Errors)
            {
                Console.WriteLine($"[Order API Validation Error] Field: {error.Key}, Error: {inner.ErrorMessage}");
            }
        }
        return new BadRequestObjectResult(context.ModelState);
    };
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = builder.Configuration["IdentityServerURL"];
        options.Audience = "resource_order";
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false
        };
    });

// DbContext
builder.Services.AddDbContext<OrderDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

// MediatR
builder.Services.AddMediatR(cfg => 
    cfg.RegisterServicesFromAssembly(typeof(CreateOrderCommandHandler).Assembly));

var rabbitMqHost = builder.Configuration["RabbitMQUrl"] ?? "localhost";
var rabbitMqPort = ushort.TryParse(builder.Configuration["RabbitMQPort"], out var parsedRabbitMqPort)
    ? parsedRabbitMqPort
    : (ushort)5672;



// MassTransit + RabbitMQ (Event consumers)
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<CourseNameChangedConsumer>();
    x.AddConsumer<PaymentCompletedConsumer>();
    x.AddConsumer<PaymentFailedConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(rabbitMqHost, rabbitMqPort, "/", host =>
        {
            host.Username("guest");
            host.Password("guest");
        });

        cfg.ReceiveEndpoint("course-name-changed-order-service", e =>
        {
            e.ConfigureConsumer<CourseNameChangedConsumer>(context);
        });
        
        cfg.ReceiveEndpoint("payment-completed-order-service", e =>
        {
            e.ConfigureConsumer<PaymentCompletedConsumer>(context);
        });
        
        cfg.ReceiveEndpoint("payment-failed-order-service", e =>
        {
            e.ConfigureConsumer<PaymentFailedConsumer>(context);
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

// Auto Migration
using (var scope = app.Services.CreateScope())
{
    var serviceProvider = scope.ServiceProvider;
    var orderDbContext = serviceProvider.GetRequiredService<OrderDbContext>();
    orderDbContext.Database.Migrate();
}

app.Run();
