using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

//
// ----------------- CONFIGURATION -----------------
//
builder.Configuration
    .AddJsonFile(
        $"configuration.{builder.Environment.EnvironmentName.ToLower()}.json",
        optional: false,
        reloadOnChange: true)
    .AddEnvironmentVariables();


//
// ----------------- SERVICES -----------------
//

// JWT Authentication for Gateway
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer("GatewayAuthenticationScheme", options =>
    {
        options.Authority = builder.Configuration["IdentityServerURL"];
        options.Audience = "resource_gateway";
        options.RequireHttpsMetadata = false;
    });

// Ocelot
builder.Services.AddOcelot();

var app = builder.Build();


//
// ----------------- MIDDLEWARE -----------------
//
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseAuthentication();
app.UseAuthorization();

// DEBUG: Log incoming token BEFORE Ocelot
app.Use(async (context, next) =>
{
    var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
    Console.WriteLine($"[Gateway] {context.Request.Method} {context.Request.Path}");
    
    if (!string.IsNullOrEmpty(authHeader))
    {
        var token = authHeader.StartsWith("Bearer ") ? authHeader.Substring(7) : authHeader;
        var dotCount = token.Count(c => c == '.');
        Console.WriteLine($"[Gateway] Token length: {token.Length}, Dot count: {dotCount}");
    }
    else
    {
        Console.WriteLine("[Gateway] No Authorization header");
    }
    
    await next();
});

// Ocelot MUST be last
await app.UseOcelot();

app.Run();
