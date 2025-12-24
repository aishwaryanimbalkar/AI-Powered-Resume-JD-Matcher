using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/match")]
public class MatchController : ControllerBase
{
    private readonly ResumeParserService _parser;
    private readonly OllamaEmbeddingService _embedder;
    private readonly SimilarityService _similarity;
    private readonly ExplanationService _explainer;

    public MatchController(ResumeParserService parser,OllamaEmbeddingService embedder,
                           SimilarityService similarity,ExplanationService explainer)
    {
        _parser = parser;
        _embedder = embedder;
        _similarity = similarity;
        _explainer = explainer;
    }

    [HttpPost]
    public async Task<IActionResult> Match(IFormFile resume,[FromForm] string jobDescription)
    {
        var resumeText = _parser.ExtractTextFromPdf(resume.OpenReadStream());

        if (resumeText.Length > 1500)
            resumeText = resumeText[..1500];

        if (jobDescription.Length > 600)
            jobDescription = jobDescription[..600];

        //var resumeEmbedding = await _embedder.GetEmbedding(resumeText);
        var jdEmbedding = await _embedder.GetEmbedding(jobDescription);
        var chunks = ChunkResume(resumeText);

        var similarities = new List<double>();

        foreach (var chunk in chunks)
        {
            if (string.IsNullOrWhiteSpace(chunk))
                continue;

            var chunkEmbedding = await _embedder.GetEmbedding(chunk);
            var sim = _similarity.Calculate(chunkEmbedding, jdEmbedding);
            similarities.Add(sim);
        }
         var finalSimilarity = similarities.Any() ? similarities.Max() : 0;
        int score = (int)Math.Round(finalSimilarity * 100);


        // var similarity = _similarity
        //     .Calculate(resumeEmbedding, jdEmbedding);

       // int score = (int)Math.Round(similarity * 100);

        var explanation = await _explainer.Explain(resumeText, jobDescription, score);

        return Ok(new
        {
            FitScore = score,
            Explanation = explanation
        });
    }

    // ================= HELPER METHODS =================

    private List<string> ChunkResume(string resume)
    {
        return new List<string>
        {
            ExtractSection(resume, "Skills"),
            ExtractSection(resume, "Experience"),
            ExtractSection(resume, "Education")
        };
    }

    private string ExtractSection(string text, string keyword)
    {
        var index = text.IndexOf(keyword, StringComparison.OrdinalIgnoreCase);
        if (index == -1)
            return string.Empty;

        var section = text[index..];
        return section.Length > 500 ? section[..500] : section;
    }
}
