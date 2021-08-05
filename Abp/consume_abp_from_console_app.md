## Consume an ABP Framework API from a .NET Core console Application

## Introduction

In this article I will show you how to connect to a protected ABP Framework API from a .NET Core console Application using the **IdentityModel.OidcClient nuget package**.

The sample **BookStore ABP Framework** application in this article has been developed with **Blazor** as UI Framework and **SQL Server** as database provider. 

The **BookStoreConsole** application is a standard **.NET Core console application**.

As I tried to keep this article as simple as possible, you will see there is still some room for code improvements.

### Source code

The source code of both projects is [available on GitHub](https://github.com/bartvanhoey/AbpAddCustomClaimToAccessToken).

## Requirements

The following tools are needed to be able to run the solution and follow along.

* .NET 6.0 SDK
* VsCode, Visual Studio 2019 Version 16.10.4+ or another compatible IDE

## ABP Framework application

### Create a new ABP Framework application

```bash
  abp new BookStore -u Blazor -o BookStore
```

### Implement the Web Application Development tutorial (part1-5)

To follow along make sure you have a protected BookAppService in the BookStore application. For this article I followed the **Web Application Development tutorial** till **part 5: Authorization**.

### Add the section below in the appsettings.json file of the DbMigrator project

```bash
"BookStore_Console": {
    "ClientId": "BookStore_Console",
    "ClientSecret": "1q2w3e*",
    "RootUrl": "https://localhost:44368"
}
```

### Add a BookStoreConsole client in the IdentityServerDataSeedContributor class of the Domain project

```bash
// BookStoreConsole Client
var bookStoreConsoleClientId = configurationSection["BookStore_Console:ClientId"];
if (!bookStoreConsoleClientId.IsNullOrWhiteSpace())
{
    var bookStoreConsoleRootUrl = configurationSection["BookStore_Console:RootUrl"].TrimEnd('/');
    await CreateClientAsync(
        name: bookStoreConsoleClientId,
        scopes: commonScopes,
        grantTypes: new[] { "password", "client_credentials""authorization_code" },
        secret: configurationSection["BookStore_Console:ClientSecret"]?.Sha256(),
        requireClientSecret: false,
        redirectUri: $"{bookStoreConsoleRootUrl}/authentication/login-callback",
    corsOrigins: new[] { bookStoreConsoleRootUrl.RemovePostFix("/") }
    );
}
```

### Run DbMigrator project

To apply the settings above you need to run the DbMigrator project. After, you can check the **IdentityServerClients** table of the database to see if the **BookStore_Console** client has been added.

## .NET Core console application

### Create a new .NET Core console application

```bash
    dotnet new console -n BookStoreConsole
```

### Install nuget packages (in terminal window or nuget package manager)

```bash
  dotnet add package IdentityModel.OidcClient --version 5.0.0-preview.1
  dotnet add package Newtonsoft.Json --version 13.0.1
```

### Add a HttpService class in the root of the project

When you want to consume a protected API the user has to be **authenticated (username+password)** and **authorized(has the right permissions)**. So, when you call the BookAppService GetListAsync method, in the header of the request you need to send the accesstoken with.

To obtain the **accesstoken** you can make use of the **nuget package IdentityModel.OidcClient**. All the heavy lifting occurs in the **GetTokensFromBookStoreApi** method (See below). These method **sends a request** to the **disco.TokenEndpoint** of the BookStoreApi and **obtains a TokenResponse**. If the correct properties are sent and the API is running, you should obtain a TokenResponse (AccessToken, IdentityToken, Scope, ...)

Afterwards the obtained accesstoken is used in the **SetBearerToken()** of the httpClient.

When you make a request now to the protected BookStore API with the httpClient, the accesstoken is sent with. The BookStore API receives this request and checks the **validity of the accesstoken** and the **permissions**. If these conditions are met, the GetListAsync method of the BookAppService returns the list of books.

```csharp
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
        client.Value.BaseAddress = new Uri("https://localhost:44388/"); //
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
            Address = disco.TokenEndpoint, // https://localhost:44388/connect/token
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
```

### Main Method

Below you see the **Main method** of the **Program.cs** file. A new HttpService gets created and the GetHttpClientAsync method is called to get a httpClient.

Next, we make a request to the BookStore API to obtain the list of books.

```csharp
static async Task Main()
{
    // if setBearerToken = false, should throw HttpRequestException: 'Response status code does not indicate success: 401 (Unauthorized).'
    // if setBearerToken = true, API should be called an list of books should be returned
    const bool setBearerToken = true;

    var httpService = new HttpService();
    var httpClient = await httpService.GetHttpClientAsync(setBearerToken);

    var response = await httpClient.Value.GetAsync("https://localhost:44388/api/app/book");
    response.EnsureSuccessStatusCode();

    var json = await response.Content.ReadAsStringAsync();

    var books = JsonConvert.DeserializeObject<ListResultDto<BookDto>>(json);

    Console.WriteLine("====================================");
    if (books?.Items != null)
        foreach (var book in books.Items)
            Console.WriteLine(book.Name);
    Console.WriteLine("====================================");
    Console.ReadKey();
}
```

## Run API and .NET Core console application

Run the **BookStore.HttpApi.Host** of the ABP Framework application first. Start the .NET Core console application next. Below is the result when the accesstoken is successfully set.

![Books returned from API](../Images/books_returned_from_api.jpg)

If you set the variable **setBearerToken** in the **Main** method to false, you will get a **401 (Unauthorized)**

![Unauthorized Exception](../Images/unauthorized_exception.jpg)

Congratulations, you can now connect to an ABP Framework API form a .NET Core console application! Check out the [source code](https://github.com/bartvanhoey/AbpAddCustomClaimToAccessToken) of this article on GitHub.

Enjoy and have fun!
