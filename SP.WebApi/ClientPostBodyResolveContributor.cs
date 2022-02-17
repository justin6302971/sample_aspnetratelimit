using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AspNetCoreRateLimit;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;

namespace SP.WebApi
{
  public class ClientPostBodyResolveContributor : IClientResolveContributor
    {
        private readonly string postBodyParamName;

        public ClientPostBodyResolveContributor(string postBodyParamName)
        {
            this.postBodyParamName = postBodyParamName;
        }

        public Task<string> ResolveClientAsync(HttpContext httpContext)
        {
            var userIdClaimFromToken = httpContext.Request.HttpContext.User.Claims.FirstOrDefault(claim => claim.Type == RateLimitEnum.UserClaimType);
            if (userIdClaimFromToken == null)
            {
                return Task.FromResult("");
            }
            //TODO:check if userid valid?

            //TODO:resolve request to certain client when it contain userid,shouldn't hard code this, but can work for now
            
            return Task.FromResult(RateLimitEnum.ClientId);
        }
    }
}