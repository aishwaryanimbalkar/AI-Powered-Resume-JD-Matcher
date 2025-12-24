using System.Text.Json.Serialization;

public class MatchResultViewModel
{
    [JsonPropertyName("fitScore")]
    public int FitScore { get; set; }

    [JsonPropertyName("explanation")]
    public string Explanation { get; set; }
}
