using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AspNetCoreRateLimit;
using IdentityModel;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;

namespace SP.WebApi
{
    public class CustomRateLimitConfiguration : RateLimitConfiguration
    {
        public override ICounterKeyBuilder EndpointCounterKeyBuilder { get; }
        public CustomRateLimitConfiguration(IOptions<IpRateLimitOptions> ipOptions, IOptions<ClientRateLimitOptions> clientOptions) : base(ipOptions, clientOptions)
        {
            EndpointCounterKeyBuilder = new CustomClientCounterKeyBuilder(clientOptions.Value);
        }

        public override void RegisterResolvers()
        {
            base.RegisterResolvers();
            ClientResolvers.Add(new ClientPostBodyResolveContributor(RateLimitEnum.BodyParam));
        }

    }

   
    public class ClientQueryStringResolveContributor : IClientResolveContributor
    {
        private readonly string queryStringParamName;

        public ClientQueryStringResolveContributor(string queryStringParamName)
        {
            this.queryStringParamName = queryStringParamName;
        }

        public Task<string> ResolveClientAsync(HttpContext httpContext)
        {
            var request = httpContext.Request;
            var queryDictionary =
                Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(
                    request.QueryString.ToString());

            string result = null;

            if (queryDictionary.ContainsKey(queryStringParamName)
                && !string.IsNullOrWhiteSpace(queryDictionary[queryStringParamName]))
            {
                result = queryDictionary[queryStringParamName];
            }
            return Task.FromResult(result);
        }
    }
}