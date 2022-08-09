using BookStoreConsole;
using BookStoreConsole.BookStoreConsole;
using Newtonsoft.Json;
using static System.Console;
using static Newtonsoft.Json.JsonConvert;

// if setBearerToken = false, should throw JsonReaderException: 'json cannot be serialized.'
// if setBearerToken = true, API should be called an list of books should be returned
const bool setBearerToken = false;

const string apiEndpoint = "https://localhost:44317/";
var httpClient = await new HttpService().GetHttpClientAsync(setBearerToken, apiEndpoint);

var response = await httpClient.Value.GetAsync($"{apiEndpoint}api/app/book");
response.EnsureSuccessStatusCode();

var json = await response.Content.ReadAsStringAsync();
try
{
    var books = DeserializeObject<ListResultDto<BookDto>>(json);
    WriteLine("====================================");
    if (books?.Items != null)
        foreach (var book in books.Items)
            WriteLine(book.Name);
    WriteLine("====================================");

}
catch (JsonReaderException)
{
    WriteLine("Deserializing went wrong");
}

