using System.IdentityModel.Tokens.Jwt;
using AspNetCoreRateLimit;
using IdentityModel;
using IdentityServer4.AccessTokenValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StackExchange.Redis;
using StackExchange.Redis.Extensions.Core.Configuration;

namespace SP.WebApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // needed to load configuration from appsettings.json
            services.AddOptions();

            // needed to store rate limit counters and rules
            services.AddMemoryCache();

            // the clientId/clientIp resolvers use it.
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            // read ClientRateLimiting setting from appsettings.json 
            services.Configure<ClientRateLimitOptions>(Configuration.GetSection("ClientRateLimiting"));

            // read ClientRateLimitPolicies  setting from appsettings.json 
            services.Configure<ClientRateLimitPolicies>(Configuration.GetSection("ClientRateLimitPolicies"));

            // inject counter and rules stores
            // services.AddDistributedRateLimiting();
            // services.AddDistributedRateLimiting<CustomProcessingStrategy>();

            // 
            // 
            var redisConfiguration = Configuration.GetSection("Redis").Get<RedisConfiguration>();
            services.AddStackExchangeRedisCache(options =>
            {
                options.ConfigurationOptions = redisConfiguration.ConfigurationOptions;
            });

            services.AddSingleton<IConnectionMultiplexer>(provider => ConnectionMultiplexer.Connect(redisConfiguration.ConfigurationOptions));

            // services.AddSingleton<IClientPolicyStore, DistributedCacheClientPolicyStore>();
            // services.AddSingleton<IRateLimitCounterStore, DistributedCacheRateLimitCounterStore>();
            //  services.AddDistributedRateLimiting();
            
            services.AddDistributedRateLimiting<RedisProcessingStrategy>();
    

            services.AddSingleton<IRateLimitConfiguration, CustomRateLimitConfiguration>();

            var identityServerConfig = Configuration.GetSection("IdentityServer").Get<IdentityServerConfig>();
            services.AddSingleton(provider => identityServerConfig);
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
            services.AddAuthentication(o =>
                {
                    o.DefaultScheme = IdentityServerAuthenticationDefaults.AuthenticationScheme;
                    o.DefaultAuthenticateScheme = IdentityServerAuthenticationDefaults.AuthenticationScheme;
                })
                .AddIdentityServerAuthentication(options =>
                {
                    options.Authority = identityServerConfig.Authority;
                    options.RequireHttpsMetadata = identityServerConfig.RequireHttpsMetadata;
                    options.ApiSecret = identityServerConfig.ApiSecret;
                    options.ApiName = identityServerConfig.ApiName;
                    options.RoleClaimType = JwtClaimTypes.Role;
                    options.NameClaimType = JwtClaimTypes.Name;
                });

            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // app.UseHttpsRedirection();

            app.UseRouting();

            // app.UseClientRateLimiting();
            // app.UseMiddleware<CustomRateLimitMiddleware<CustomClientRateLimitProcessor>>();

            app.UseAuthentication();
            app.UseAuthorization();
            app.UseMiddleware<CustomUserClientRateLimitMiddleware>();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers().RequireAuthorization();
            });
        }
    }
}
