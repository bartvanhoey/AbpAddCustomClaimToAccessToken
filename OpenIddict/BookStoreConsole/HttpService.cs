using IdentityModel.Client;

namespace BookStoreConsole
{
    public class HttpService
    {
        public async Task<Lazy<HttpClient>> GetHttpClientAsync(bool setBearerToken, string apiEndpoint)
        {
            var client = new Lazy<HttpClient>(() => new HttpClient());

            if (setBearerToken) client.Value.SetBearerToken(await GetAccessToken(apiEndpoint));
            
            client.Value.BaseAddress = new Uri(apiEndpoint); 
            return await Task.FromResult(client);
        }
        private static async Task<TokenResponse> GetTokensFromBookStoreApi(string apiEndpoint)
        {
            var discoveryCache = new DiscoveryCache(apiEndpoint);
            var disco = await discoveryCache.GetAsync();
            var httpClient = new Lazy<HttpClient>(() => new HttpClient());
            var response = await httpClient.Value.RequestPasswordTokenAsync(new PasswordTokenRequest
            {
                Address = disco.TokenEndpoint, // apiEndpoint/connect/token
                ClientId = "BookStore_Console",
                ClientSecret = "1q2w3e*",
                UserName = "admin",
                Password = "1q2w3E*",
                Scope = "openid offline_access address email phone profile roles BookStore",
            });
            return response.IsError ?  new TokenResponse() : response;
        }
        private static async Task<string> GetAccessToken(string apiEndpoint) => (await GetTokensFromBookStoreApi(apiEndpoint)).AccessToken;
    }
}
