using System.Text.Json.Serialization;

public class ListResultDto<T>
{
    [JsonPropertyName("items")]
    public IReadOnlyList<T> Items { get; set; } = [];
}


