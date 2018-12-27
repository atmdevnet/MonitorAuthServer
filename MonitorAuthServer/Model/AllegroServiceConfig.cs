using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace MonitorAuthServer.Model
{
    public class AllegroServiceConfig
    {
        public string Host { get; set; }
        public string Path { get; set; }
        public AllegroServiceAuthorityConfig Authority { get; set; }
    }

    public class AllegroServiceAuthorityConfig
    {
        public string Url { get; set; }
        public AuthorityRequest RequestContent { get; set; }
    }

    public class AuthorityRequest
    {
        [JsonProperty("client_id")]
        public string ClientId { get; set; }
        [JsonProperty("client_secret")]
        public string ClientSecret { get; set; }
        [JsonProperty("audience")]
        public string Audience { get; set; }
        [JsonProperty("grant_type")]
        public string GrantType { get; set; }
    }

    public class AuthorityToken
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }
        [JsonProperty("expires_in")]
        public int ExpiresIn { get; set; }
        [JsonProperty("token_type")]
        public string TokenType { get; set; }
    }
}
