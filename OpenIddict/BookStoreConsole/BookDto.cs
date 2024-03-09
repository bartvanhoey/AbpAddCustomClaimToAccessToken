using System.Text.Json.Serialization;

public class BookDto
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }
    [JsonPropertyName("publishDate")]
    public DateTime PublishDate { get; set; }
    [JsonPropertyName("type")]
    public BookType Type { get; set; }
    [JsonPropertyName("price")]
    public float Price { get; set; }
}


