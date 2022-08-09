using IdentityModel.Client;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace BookStoreConsole
{
    public class HttpService
    {
        public async Task<Lazy<HttpClient>> GetHttpClientAsync(bool setBearerToken)
        {
            var client = new Lazy<HttpClient>(() => new HttpClient());
            var accessToken = await GetAccessToken();
            if (setBearerToken)
            {
                client.Value.SetBearerToken(accessToken);
            }
            client.Value.BaseAddress = new Uri("https://localhost:44388/");
            return await Task.FromResult(client);
        }

        private static async Task<TokenResponse> GetTokensFromBookStoreApi()
        {
            var authority = "https://localhost:44388/";
            var discoveryCache = new DiscoveryCache(authority);
            var disco = await discoveryCache.GetAsync();
            var httpClient = new Lazy<HttpClient>(() => new HttpClient());
            var response = await httpClient.Value.RequestPasswordTokenAsync(new PasswordTokenRequest
            {
                Address = disco.TokenEndpoint,
                ClientId = "BookStore_Console",
                ClientSecret = "1q2w3e",
                UserName = "admin",
                Password = "1q2w3E*",
                Scope = "email openid profile role phone address BookStore",
            });
            if (response.IsError) throw new Exception(response.Error);
            return response;
        }

        private async Task<string> GetAccessToken()
        {
            var accessToken = (await GetTokensFromBookStoreApi()).AccessToken;
            return accessToken;
        }

    }
}
