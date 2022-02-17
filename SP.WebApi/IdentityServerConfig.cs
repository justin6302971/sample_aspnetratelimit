namespace SP.WebApi
{
    public class IdentityServerConfig
    {
        public string Authority { get; set; }
        public bool RequireHttpsMetadata { get; set; }
        public string ApiSecret { get; set; }
        public string ApiName { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string Scope { get; set; }
    }
}