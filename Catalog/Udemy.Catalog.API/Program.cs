using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Udemy.Catalog.API.Options;
using Udemy.Catalog.API.Services.Abstract;
using Udemy.Catalog.API.Services.Concrete;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers(options =>
{
    options.Filters.Add(new AuthorizeFilter());
});

System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpContextAccessor();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.Authority = builder.Configuration["IdentityServerURL"];
    options.Audience = "resource_catalog";
    options.RequireHttpsMetadata = false;
});

builder.Services.AddAutoMapper(typeof(Program));

builder.Services.Configure<DatabaseOptions>(builder.Configuration.GetSection("DatabaseOptions"));

var rabbitMqHost = builder.Configuration["RabbitMQUrl"] ?? "localhost";
var rabbitMqPort = ushort.TryParse(builder.Configuration["RabbitMQPort"], out var parsedRabbitMqPort)
    ? parsedRabbitMqPort
    : (ushort)5672;

// MassTransit + RabbitMQ (CourseNameChanged event publish için)
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

builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<ICourseService, CourseService>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Seed Data
using (var scope = app.Services.CreateScope())
{
    var serviceProvider = scope.ServiceProvider;
    await Udemy.Catalog.API.Services.DatabaseSeedHelper.SeedCategoriesAsync(serviceProvider);
}

// app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
