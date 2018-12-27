using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace MonitorAuthServer.Model
{
    public interface IAllegroService
    {
        Task<DateTime?> GetSystemTime();
        Task<(
                long id,
                string login,
                int country,
                int rating,
                DateTime created,
                DateTime lastLogin,
                bool blocked,
                bool closed,
                bool terminated,
                bool shop,
                bool standard,
                bool newbie,
                bool junior,
                bool notActivated
            )?> GetUser(long? id, string login);
        string[] Errors { get; }
    }


    public class AllegroServiceFactory
    {
        private AllegroServiceConfig _config = null;
        private AuthorityToken _token = null;
        private Uri _uri = null;

        public static AllegroServiceFactory Create(IOptions<AllegroServiceConfig> config)
        {
            AllegroServiceFactory result = null;

            Task.Run<AllegroServiceFactory>(async () => 
            {
                var token = await requestToken(config.Value.Authority);
                return new AllegroServiceFactory(config.Value, token);
            })
            .ContinueWith(t => 
            {
                if (!t.IsFaulted)
                {
                    result = t.Result;
                }
            })
            .Wait();

            return result;
        }

        private AllegroServiceFactory(AllegroServiceConfig config, AuthorityToken token)
        {
            _config = config;
            _token = token;
            _uri = getServiceUri();
        }


        public IAllegroService Create()
        {
            return new AllegroService(_token, _uri);
        }


        private async static Task<AuthorityToken> requestToken(AllegroServiceAuthorityConfig config)
        {
            AuthorityToken token = null;

            using (var client = new HttpClient())
            {
                var contentJson = Newtonsoft.Json.JsonConvert.SerializeObject(config.RequestContent);

                using (var content = new StringContent(contentJson))
                {
                    content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                    using (var response = await client.PostAsync(config.Url, content))
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            var tokenJson = await response.Content.ReadAsStringAsync();
                            token = Newtonsoft.Json.JsonConvert.DeserializeObject<AuthorityToken>(tokenJson);
                        }
                    }
                }
            }

            return token;
        }

        private Uri getServiceUri()
        {
            return new Uri(new Uri(_config.Host), _config.Path);
        }



        /// <summary>
        /// service implementation
        /// </summary>
        private class AllegroService : IAllegroService
        {
            private AuthorityToken _token = null;
            private Uri _uri = null;

            public string[] Errors { get; private set; }

            public AllegroService(AuthorityToken token, Uri uri)
            {
                _token = token;
                _uri = uri;
            }

            public async Task<DateTime?> GetSystemTime()
            {
                DateTime? result = null;

                var uri = new UriBuilder(_uri);
                uri.Path += "/systime";

                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(_token.TokenType, _token.AccessToken);

                    using (var response = await client.GetAsync(uri.Uri))
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            var content = await response.Content.ReadAsStringAsync();

                            var data = Newtonsoft.Json.JsonConvert.DeserializeAnonymousType(content, new { systime = "" });
                            result = DateTime.TryParseExact(data.systime, "yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out DateTime systime) ? systime : default(DateTime?);
                        }
                        else
                        {
                            Errors = await readErrors(response.Content);
                        }
                    }
                }

                return result;
            }

            public async Task<(
                long id,
                string login,
                int country,
                int rating,
                DateTime created,
                DateTime lastLogin,
                bool blocked,
                bool closed,
                bool terminated,
                bool shop,
                bool standard,
                bool newbie,
                bool junior,
                bool notActivated
            )?> GetUser(long? id, string login)
            {
                var uri = new UriBuilder(_uri);
                uri.Path += "/user/info";

                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(_token.TokenType, _token.AccessToken);

                    var payloadData = new { id = id.HasValue ? id.Value : 0L, login = login };
                    var payloadJson = Newtonsoft.Json.JsonConvert.SerializeObject(payloadData);

                    using (var payload = new StringContent(payloadJson))
                    {
                        payload.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                        using (var response = await client.PostAsync(uri.Uri, payload))
                        {
                            if (response.IsSuccessStatusCode)
                            {
                                var content = await response.Content.ReadAsStringAsync();

                                var data = Newtonsoft.Json.JsonConvert.DeserializeAnonymousType(content, new
                                {
                                    id = 0L,
                                    login = "",
                                    country = 0,
                                    rating = 0,
                                    created = DateTime.UtcNow,
                                    lastLogin = DateTime.UtcNow,
                                    blocked = false,
                                    closed = false,
                                    terminated = false,
                                    shop = false,
                                    standard = false,
                                    newbie = false,
                                    junior = false,
                                    notActivated = false
                                });

                                return (
                                    data.id,
                                    data.login,
                                    data.country,
                                    data.rating,
                                    data.created,
                                    data.lastLogin,
                                    data.blocked,
                                    data.closed,
                                    data.terminated,
                                    data.shop,
                                    data.standard,
                                    data.newbie,
                                    data.junior,
                                    data.notActivated
                                    );
                            }
                            else
                            {
                                Errors = await readErrors(response.Content);
                            }
                        }
                    }
                }

                return null;
            }

            private async Task<string[]> readErrors(HttpContent content)
            {
                var contentJson = await content.ReadAsStringAsync();
                var data = Newtonsoft.Json.JsonConvert.DeserializeAnonymousType(contentJson, new { errors = new string[0] });

                return data.errors;
            }
        }
    }
}
