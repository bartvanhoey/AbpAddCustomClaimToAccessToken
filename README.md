## Consume ABP Framework API from .NET Core Console Application

## Introduction

In this article I will show you can connect to a protected ABP Framework API from a .NET Core Console Application using the IdentityModel.OidcClient nuget package.

The BookStore ABP Framework application has been developed with Blazor as UI framework and SQL Server as database provider. The BookStoreConsole application is a standard .NET Core console application.

As I tried to keep this article as simple as possible, there is still some room for code refactoring.

### Source code

The Source code of the completed application is [available on GitHub](https://github.com/bartvanhoey/AbpAddCustomClaimToAccessToken).

## Requirements

The following tools are needed to be able to run the solution and follow along.

* .NET 6.0 SDK
* VsCode, Visual Studio 2019 Version 16.10.4+ or another compatible IDE

## ABP Framework application

### Create a new ABP Framework application

```bash
  abp new BookStore -u Blazor -o BookStore
```

### Implement the BookStore tutorial (part1-5)

To follow along make sure you have a protected BookAppService in the ABP Framework application. For this article I followed the BookStore tutorial till part 5.

### Add section below in appsettings.json from DbMigrator project

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

## Create a .NET Core console application

### Run the command below to create a new .NET Core console application

```bash
    dotnet new console -n BookStoreConsole
```

### Install nuget packages (in terminal window of nuget package manager)

```bash
  dotnet add package IdentityModel.OidcClient --version 5.0.0-preview.1
  dotnet add package Newtonsoft.Json --version 13.0.1
```

### Add an HttpService class in the root of the project with the code below

When you want to consume a protected API (the AbpAuthorizeAttribute is set) the user has to be authenticated and authorized. So, when you call the BookAppService GetListAsync method, in the header of the request you need to send the accesstoken.

To obtain the **accesstoken** you can make use of the nuget package IdentityModel.OidcClient. All the heavy lifting occurs in the **GetTokensFromBookStoreApi** method. These method sends a request to the disco.TokenEndpoint of the BookStoreApi and obtains a TokenResponse. If the correct properties are sent and the API is running, you should obtain a TokenResponse (AccessToken, IdentityToken, Scope, ...)

Afterwards the obtained accesstoken is used in the **SetBearerToken()** of the httpclient. 

When you make a request now to the protected BookStore API with the httpclient, the accesstoken is sent with. When the BookStore API receives the request, it checks the accesstoken, and when valid + correct permission, the GetListAsync method of the BookAppService returns a list of books.

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




### Open & Run the Application
* Open the solution in Visual Studio (or your favorite IDE).
* Run the `AbpFileUploadToAzureStorage.DbMigrator` application to apply the migrations and seed the initial data.
* Run the `AbpFileUploadToAzureStorage.HttpApi.Host` application to start the server side.
* Run the `AbpFileUploadToAzureStorage.Blazor` application to start the Blazor UI project.
## Development
### Install Volo.Abp.BlobStoring.Azure NuGet package to Domain project
* Open a command prompt in the directory of the **Domain** project.
* Run the command below to install **Volo.Abp.BlobStoring.Azure** NuGet package
  
```bash
  abp add-package Volo.Abp.BlobStoring.Azure
```
### Create a class AzureStorageAccountOptions to retrieve Azure Storage settings
* Create an **AzureStorage** folder in the **Domain** project of your application.
* Add a **AzureStorageAccountOptions.cs** file to the **AzureStorage** folder.
```csharp
namespace AbpFileUploadToAzureStorage
{
  public class AzureStorageAccountOptions
  {
    public string ConnectionString { get; set; }
    public string AccountUrl { get; set; }
  }
}
```
### Add AzureStorageAccountSettings to the appsettings.json file in the HttpApi.Host project
```json
{
  // other settings here
  "AzureStorageAccountSettings" : {
    "ConnectionString" : "AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;DefaultEndpointsProtocol=http;BlobEndpoint=http://127.0.0.1:10000/devstoreaccount1;QueueEndpoint=http://127.0.0.1:10001/devstoreaccount1;TableEndpoint=http://127.0.0.1:10002/devstoreaccount1;",
    "AccountUrl" : "http://127.0.0.1:10000/devstoreaccount1/"
  }
}
```
The connection string above serves as connection to Azurite (local Azure Storage emulator). You will need to replace the connection string when you want to upload files to Azure Storage in the cloud.
### Create a ConfigureAzureStorageAccountOptions method in the AbpFileUploadToAzureStorageDomainModule class of the Domain project
```csharp
    // Comment out and place in using section page
    // using Microsoft.Extensions.Configuration;
    
    private void ConfigureAzureStorageAccountOptions(ServiceConfigurationContext context, IConfiguration configuration)
    {
      Configure<AzureStorageAccountOptions>(options =>
      {
        var azureStorageConnectionString = configuration["AzureStorageAccountSettings:ConnectionString"];
        var azureStorageAccountUrl = configuration["AzureStorageAccountSettings:AccountUrl"];
        options.ConnectionString = azureStorageConnectionString;
        options.AccountUrl = azureStorageAccountUrl;
      });
    }
```
### Call the ConfigureAzureStorageAccountOptions method from the ConfigureServices method in the AbpFileUploadToAzureStorageDomainModule
```csharp
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
      // ...
      var configuration = context.Services.GetConfiguration();
      ConfigureAzureStorageAccountOptions(context, configuration);
    }
```
### Add a PizzaPictureContainer class in the Domain project
* Add a **PizzaPictureContainer.cs** file to the **AzureStorage/Pizzas** folder of the **Domain** project.
```csharp
using Volo.Abp.BlobStoring;
namespace AbpFileUploadToAzureStorage.Domain.AzureStorage
{
    [BlobContainerName("pizza-picture-container")]
    public class PizzaPictureContainer
    {
        
    }
}
```
### Create a ConfigureAbpBlobStoringOptions method in the AbpFileUploadToAzureStorageDomainModule of the Domain project
```csharp
    // Comment out and place in using section page
    // using Volo.Abp.BlobStoring;
    // using AbpFileUploadToAzureStorage.Domain.AzureStorage;
    private void ConfigureAbpBlobStoringOptions(IConfiguration configuration)
    {
      Configure<AbpBlobStoringOptions>(options =>
      {
        var azureStorageConnectionString = configuration["AzureStorageAccountSettings:ConnectionString"];
        options.Containers.Configure<PizzaPictureContainer>(container =>
        {
          container.UseAzure(azure =>
                {
                  azure.ConnectionString = azureStorageConnectionString;
                  azure.CreateContainerIfNotExists = true;
                });
        });
      });
    }
```
### Call the ConfigureAbpBlobStoringOptions method from the ConfigureServices method in the AbpFileUploadToAzureStorageDomainModule
```csharp
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
      // ...
      var configuration = context.Services.GetConfiguration();
      ConfigureAzureStorageAccountOptions(context, configuration);
      
      ConfigureAbpBlobStoringOptions(configuration);
    }
```
### Create PizzaPictureContainerManager class in folder AzureStorage/Pizzas of the Domain project
```csharp
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Volo.Abp.BlobStoring;
using Volo.Abp.Domain.Services;
namespace AbpFileUploadToAzureStorage.Domain.AzureStorage.Pizzas
{
  public class PizzaPictureContainerManager : DomainService
  {
    private readonly IBlobContainer<PizzaPictureContainer> _pizzaPictureContainer;
    private readonly AzureStorageAccountOptions _azureStorageAccountOptions;
    public PizzaPictureContainerManager(IBlobContainer<PizzaPictureContainer> pizzaPictureContainer, IOptions<AzureStorageAccountOptions> azureStorageAccountOptions)
    {
      _pizzaPictureContainer = pizzaPictureContainer;
      _azureStorageAccountOptions = azureStorageAccountOptions.Value;
    }
    public async Task<string> SaveAsync(string fileName, byte[] byteArray, bool overrideExisting = false)
    {
      var extension = Path.GetExtension(fileName);
      var storageFileName = $"{Path.GetFileNameWithoutExtension(fileName)}_{Guid.NewGuid()}{extension}";
      await _pizzaPictureContainer.SaveAsync(storageFileName, byteArray, overrideExisting);
      return storageFileName;
    }
  }
}
```
### Add IPizzaAppService, SavePizzaPictureDto and SavedPizzaPictureDto to the Application.Contracts project
```csharp
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
namespace AbpFileUploadToAzureStorage.Application.Contracts.AzureStorage.Pizzas
{
  public interface IPizzaAppService : IApplicationService
  {
    Task<SavedPizzaPictureDto> SavePizzaPicture(SavePizzaPictureDto input);
  }
}
```
```csharp
namespace AbpFileUploadToAzureStorage.Application.Contracts.AzureStorage.Pizzas
{
  public class SavePizzaPictureDto
  {
    public string FileName { get; set; }
    public byte[] Content { get; set; }
  }
}
```
```csharp
namespace AbpFileUploadToAzureStorage.Application.Contracts.AzureStorage.Pizzas
{
  public class SavedPizzaPictureDto
  {
    public string StorageFileName { get; set; }
  }
}
```
### Add a PizzaAppService class in the Application project and implement IPizzaAppService interface
```csharp
using System.Threading.Tasks;
using AbpFileUploadToAzureStorage.Application.Contracts.AzureStorage.Pizzas;
using AbpFileUploadToAzureStorage.Domain.AzureStorage.Pizzas;
using Volo.Abp.Application.Services;
namespace AbpFileUploadToAzureStorage.Application.AzureStorage.Pizzas
{
  public class PizzaAppService : ApplicationService, IPizzaAppService
  {
    private readonly PizzaPictureContainerManager _pizzaPictureContainerManager;
    public PizzaAppService(PizzaPictureContainerManager pizzaPictureContainerManager)
    {
      _pizzaPictureContainerManager = pizzaPictureContainerManager;
    }
    public async Task<SavedPizzaPictureDto> SavePizzaPicture(SavePizzaPictureDto input)
    {
      var storageFileName = await _pizzaPictureContainerManager.SaveAsync(input.FileName, input.Content, true);
      return new SavedPizzaPictureDto { StorageFileName = storageFileName };
    }
  }
}
```
### Replace content of Index.razor with code below
```html
@page "/"
@inherits AbpFileUploadToAzureStorageComponentBase
<div class="container">
    <CardDeck>
        <div class="card mt-4 mb-5">
            <div class="card-body">
                <div class="col-lg-12">
                    <div class="p-12">
                        <h5><i class="fas fa-file-upload text-secondary pr-2 my-2 fa-2x"></i>Upload
                            File to Azure Storage
                        </h5>
                        <p>
                            <InputFile OnChange="@OnInputFileChange" />
                        </p>
                    </div>
                </div>
            </div>
        </div>
        <div class="card mt-4 mb-5">
            <div class="card-body">
                <CardImage Source="@PictureUrl" Alt="Pizza picture will be displayed here!"></CardImage>
            </div>
        </div>
    </CardDeck>
</div>
```
### Replace content of Index.razor.cs with code below
```csharp
using System;
using System.Linq;
using System.Threading.Tasks;
using AbpFileUploadToAzureStorage.Application.Contracts.AzureStorage.Pizzas;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
namespace AbpFileUploadToAzureStorage.Blazor.Pages
{
  public partial class Index
  {
    [Inject] protected IPizzaAppService PizzaAppService { get; set; }
    public SavedPizzaPictureDto SavedPizzaPictureDto { get; set; } = new SavedPizzaPictureDto();
    protected string PictureUrl;
    protected async Task OnInputFileChange(InputFileChangeEventArgs e)
    {
      var file = e.GetMultipleFiles(1).FirstOrDefault();
      var byteArray = new byte[file.Size];
      await file.OpenReadStream().ReadAsync(byteArray);
      SavedPizzaPictureDto = await PizzaAppService.SavePizzaPicture(new SavePizzaPictureDto { Content = byteArray, FileName = file.Name }); ;
      var format = "image/png";
      var imageFile = (e.GetMultipleFiles(1)).FirstOrDefault();
      var resizedImageFile = await imageFile.RequestImageFileAsync(format, 100, 100);
      var buffer = new byte[resizedImageFile.Size];
      await resizedImageFile.OpenReadStream().ReadAsync(buffer);
      PictureUrl = $"data:{format};base64,{Convert.ToBase64String(buffer)}";
    }
  }
}
```
## Test uploading a picture to Azure Storage
* Start both the Blazor and the HttpApi.Host project.
* Choose a pizza picture in the **File Upload to Azure Storage** section.
  
![Upload file to Azure Storage](images/index.jpg)
Et voilà! As you can see in the **Azure Storage Explorer**, the pizza picture has been successfully stored in **Azure Storage**.
![File uploaded to  Azure Storage](images/pizza_in_azure_storage_explorer.jpg)
Congratulations, you can upload a file to Azure Storage by now! Check out the source code of this article to see my implementation of Uploading/Deleting a file to Azure Storage.
Find more info about **Blob Storing** [here](https://docs.abp.io/en/abp/latest/Blob-Storing) and **BLOB Storing Azure Provider** [here](https://docs.abp.io/en/abp/latest/Blob-Storing-Azure).
Get the [source code](https://github.com/bartvanhoey/AbpFileUploadToAzureStorage) on GitHub.
Enjoy and have fun!
