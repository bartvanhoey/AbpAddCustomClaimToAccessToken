using System.Text.Json;
using static System.Console;

// if setBearerToken = false, should throw JsonReaderException: 'json cannot be serialized.'
// if setBearerToken = true, API should be called an list of books should be returned
const bool setBearerToken = false;
const string apiEndpoint = "https://localhost:44388/";

try
{
    var httpClient = await new HttpService().GetHttpClientAsync(setBearerToken, apiEndpoint);

    var response = await httpClient.Value.GetAsync($"{apiEndpoint}api/app/book");
    response.EnsureSuccessStatusCode();

    var json = await response.Content.ReadAsStringAsync();

    ListResultDto<BookDto>? books = new();


    books = JsonSerializer.Deserialize<ListResultDto<BookDto>>(json);
    WriteLine("====================================");
    if (books?.Items != null)
        foreach (var book in books.Items)
            WriteLine(book.Name);

}
catch (HttpRequestException)
{
    WriteLine("Is apiEndpoint correct?");
}
catch (JsonException)
{
    WriteLine("setBearerToken to true");
}
WriteLine("====================================");



