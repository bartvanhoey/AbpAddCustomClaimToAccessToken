// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace BookStoreConsole
{
    public class BookDto
    {
        public string? Name { get; set; }
        public DateTime PublishDate { get; set; }

        public BookType Type { get; set; }
        public float Price { get; set; }
    }
}
