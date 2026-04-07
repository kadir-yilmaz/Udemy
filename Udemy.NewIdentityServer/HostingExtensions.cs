using Duende.IdentityServer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Udemy.NewIdentityServer.Data;
using Udemy.NewIdentityServer.Models;
using Udemy.NewIdentityServer.Services;

namespace Udemy.NewIdentityServer
{
    internal static class HostingExtensions
    {
        public static WebApplication ConfigureServices(this WebApplicationBuilder builder)
        {
            builder.Services.AddRazorPages();
            builder.Services.AddControllers();

            // Local API Authentication (for UsersController)
            builder.Services.AddLocalApiAuthentication();

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            builder.Services
                .AddIdentityServer(options =>
                {
                    options.Events.RaiseErrorEvents = true;
                    options.Events.RaiseInformationEvents = true;
                    options.Events.RaiseFailureEvents = true;
                    options.Events.RaiseSuccessEvents = true;
                    options.IssuerUri = "http://identityserver:8080";
                    
                    // see https://docs.duendesoftware.com/identityserver/v6/fundamentals/resources/
                    options.EmitStaticAudienceClaim = true;

                    // Automatic Key Management (ücretli özellik) devre dışı bırakılıyor
                    options.KeyManagement.Enabled = false;
                })
                .AddInMemoryIdentityResources(Config.IdentityResources)
                .AddInMemoryApiResources(Config.ApiResources)
                .AddInMemoryApiScopes(Config.ApiScopes)
                .AddInMemoryClients(Config.Clients)
                // SQL Server ile kalıcı grant store (refresh token, authorization code, consent)
                .AddOperationalStore(options =>
                {
                    options.ConfigureDbContext = b =>
                        b.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
                            sql => sql.MigrationsAssembly(typeof(Program).Assembly.GetName().Name));

                    // Süresi dolan tokenları otomatik temizle
                    options.EnableTokenCleanup = true;
                    options.TokenCleanupInterval = 3600; // 1 saat (saniye cinsinden)
                })
                .AddAspNetIdentity<ApplicationUser>()
                .AddResourceOwnerValidator<IdentityResourceOwnerPasswordValidator>()
                .AddProfileService<IdentityProfileService>()
                .AddDeveloperSigningCredential();

            return builder.Build();
        }

        public static WebApplication ConfigurePipeline(this WebApplication app)
        {
            app.UseSerilogRequestLogging();

            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles();
            app.UseRouting();
            app.UseIdentityServer();
            app.UseAuthorization();

            app.MapRazorPages()
                .RequireAuthorization();

            app.MapControllers();

            return app;
        }
    }
}