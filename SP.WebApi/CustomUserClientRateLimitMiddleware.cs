using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AspNetCoreRateLimit;
using IdentityModel;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;

namespace SP.WebApi
{
    public class CustomUserClientRateLimitMiddleware : RateLimitMiddleware<ClientRateLimitProcessor>
    {
        private readonly ILogger<CustomUserClientRateLimitMiddleware> _logger;
        private readonly IRateLimitConfiguration _config;

        public CustomUserClientRateLimitMiddleware(RequestDelegate next,
            IProcessingStrategy processingStrategy,
            IOptions<ClientRateLimitOptions> options,
            IRateLimitCounterStore counterStore,
            IClientPolicyStore policyStore,
            IRateLimitConfiguration config,
            ILogger<CustomUserClientRateLimitMiddleware> logger)
        : base(next, options?.Value, new ClientRateLimitProcessor(options?.Value, counterStore, policyStore, config, processingStrategy), config)
        {
            _logger = logger;
            _config = config;
        }

        public override async Task<ClientRequestIdentity> ResolveIdentityAsync(HttpContext httpContext)
        {
            string clientIp = null;
            string clientId = null;

            if (_config.ClientResolvers?.Any() == true)
            {
                foreach (var resolver in _config.ClientResolvers)
                {
                    clientId = await resolver.ResolveClientAsync(httpContext);

                    if (!string.IsNullOrEmpty(clientId))
                    {
                        break;
                    }
                }
            }

            if (_config.IpResolvers?.Any() == true)
            {
                foreach (var resolver in _config.IpResolvers)
                {
                    clientIp = resolver.ResolveIp(httpContext);

                    if (!string.IsNullOrEmpty(clientIp))
                    {
                        break;
                    }
                }
            }
            var path = httpContext.Request.Path.ToString().ToLowerInvariant();

            string userId = "";
            var userIdClaimFromToken = httpContext.Request.HttpContext.User.Claims.FirstOrDefault(claim => claim.Type == JwtClaimTypes.Subject);
            if (userIdClaimFromToken != null)
            {
                userId = userIdClaimFromToken.Value;
            }

            // httpContext.Request.EnableBuffering();
            // var reader = new StreamReader(httpContext.Request.Body);
            // string requestContent = await reader.ReadToEndAsync();
            // httpContext.Request.Body.Seek(0, SeekOrigin.Begin);


            // var userId =   httpContext.Request.User.Identity.Claims.First(claim => claim.Type == JwtClaimTypes.Subject).Value;


            // if (!string.IsNullOrEmpty(requestContent))
            // {
            //     var jobjectBody = JObject.Parse(requestContent);
            //     var bodyParam = jobjectBody.GetValue(nameof(userId), StringComparison.OrdinalIgnoreCase)?.Value<string>();
            //     if (!string.IsNullOrEmpty(bodyParam))
            //     {
            //         userId = bodyParam;
            //     }
            // }
            return new CustomClientRequestIdentity
            {
                ClientIp = clientIp,
                Path = path == "/"
                    ? path
                    : path.TrimEnd('/'),
                HttpVerb = httpContext.Request.Method.ToLowerInvariant(),
                ClientId = clientId ?? "anon",
                UserId = userId
            };

        }

        protected override void LogBlockedRequest(HttpContext httpContext, ClientRequestIdentity identity, RateLimitCounter counter, RateLimitRule rule)
        {
            _logger.LogInformation($"Request {identity.HttpVerb}:{identity.Path} from ClientId {identity.ClientId} has been blocked, quota {rule.Limit}/{rule.Period} exceeded by {counter.Count - rule.Limit}. Blocked by rule {rule.Endpoint}, TraceIdentifier {httpContext.TraceIdentifier}.");
        }
    }

    public class CustomClientRequestIdentity : ClientRequestIdentity
    {
        public string UserId { get; set; }
    }
}