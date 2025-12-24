using System.Net.Http.Json;

public class OllamaEmbeddingService
{

    private readonly HttpClient _client;

    public OllamaEmbeddingService(IHttpClientFactory factory)
    {
        _client = factory.CreateClient("OllamaClient");
    }

    public async Task<float[]> GetEmbedding(string text)
    {
        var response = await _client.PostAsJsonAsync("/api/embeddings",
            new
            {
                model = "nomic-embed-text",
                prompt = text
            });

        var result = await response.Content.ReadFromJsonAsync<EmbeddingResponse>();

        return result.embedding;
    }
}
