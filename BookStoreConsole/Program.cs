using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace BookStoreConsole
{
    class Program
    {
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
    }
}