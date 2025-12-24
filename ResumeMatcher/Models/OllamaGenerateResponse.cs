// public class OllamaGenerateResponse
// {
//     public string response { get; set; }
// }
using System.Text.Json.Serialization;

public class OllamaGenerateResponse
{
    [JsonPropertyName("response")]
    public string Response { get; set; }
}
