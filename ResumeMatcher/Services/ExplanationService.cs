using System.Net.Http.Json;
using System.Text.Json;

public class ExplanationService
{

    private readonly HttpClient _client;

    public ExplanationService(IHttpClientFactory factory)
    {
        _client = factory.CreateClient("OllamaClient");
    }

public async Task<string> Explain(string resume, string jd, int score)
{
if (resume.Length > 600) resume = resume[..600];
    if (jd.Length > 400) jd = jd[..400];

// var prompt = $"""
// You are an AI hiring assistant.

// Analyze the resume against the job description.

// Job Description:
// {jd}

// Resume:
// {resume}

// // IMPORTANT RULES:
// // - Return ONLY one short sentence
// STRICT RULES:
// - Output EXACTLY one sentence
// - Format MUST be: "<strength> but lacks <missing skill>"
// - Max 12 words
// - No quotes
// - No headings
// - No explanation text

// // Example output:
// // Strong backend experience but lacks cloud skills.
// """;
var prompt = $"""
TASK:
Generate exactly ONE short sentence comparing a resume to a job description.

STRICT RULES (MANDATORY):
- DO NOT summarize the resume
- DO NOT describe the candidate
- DO NOT include names
- DO NOT infer skills
- Use ONLY skills explicitly present in the resume text
- Skills present only in the job description MUST be treated as missing

RESUME (source of truth):
{resume}

JOB DESCRIPTION (used only to find missing skills):
{jd}

OUTPUT FORMAT (MUST FOLLOW EXACTLY):
<resume strength> but lacks <missing job skill>

CONSTRAINTS:
- Exactly ONE sentence
- Max 12 words
- No quotes
- No headings
- No extra text

VALID OUTPUT EXAMPLES:
Strong backend experience but lacks cloud deployment skills.
Good ASP.NET Core skills but lacks Azure experience.
""";

    var response = await _client.PostAsJsonAsync("/api/generate",
        new
        {
            model = "llama3",
            prompt = prompt,
            stream = false,
            options = new
            {
                temperature = 0.0,
                num_predict = 15,
                num_ctx = 512
            }
        });

    response.EnsureSuccessStatusCode();

    var rawJson = await response.Content.ReadAsStringAsync();
    var result = JsonSerializer.Deserialize<OllamaGenerateResponse>(rawJson);

    return result?.Response;
}

}
