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
// TASK:
// Generate exactly ONE short sentence comparing a resume to a job description.

// STRICT RULES (MANDATORY):
// - DO NOT summarize the resume
// - DO NOT describe the candidate
// - DO NOT include names
// - DO NOT infer skills
// - Use ONLY skills explicitly present in the resume text
// - Skills present only in the job description MUST be treated as missing

// RESUME (source of truth):
// {resume}

// JOB DESCRIPTION (used only to find missing skills):
// {jd}

// OUTPUT FORMAT (MUST FOLLOW EXACTLY):
// <resume strength> but lacks <missing job skill>

// CONSTRAINTS:
// - Exactly ONE sentence
// - Max 12 words
// - No quotes
// - No headings
// - No extra text

// VALID OUTPUT EXAMPLES:
// Strong backend experience but lacks cloud deployment skills.
// Good ASP.NET Core skills but lacks Azure experience.
// """;
var prompt = $"""
[INST] <<SYS>>
You are a STRICT deterministic ATS skill comparison engine.
You must output ONLY the final answer sentence.
You must NOT infer, assume, generalize, abstract, or rename skills.
You must NEVER convert skills into job roles (e.g., ".NET Developer").
You must NEVER invent missing experience.
Only literal skill text comparison is allowed.
<</SYS>>

JOB DESCRIPTION (JD):
{jd}

CANDIDATE RESUME:
{resume}

TASK:
Compare JD-required skills with Resume skills.

MATCHING RULES (MANDATORY):
1. Extract ONLY explicit technical skills mentioned in the JD.
2. A skill is a MATCH only if the SAME skill text appears explicitly in the Resume.
3. A skill is a LACK only if it appears in the JD but NOT in the Resume.
4. Ignore ALL Resume skills not mentioned in the JD.
5. Resume-only skills must NEVER appear in the output.

OUTPUT CONTRACT (ABSOLUTE):
- Output exactly ONE sentence and NOTHING else.
- Start with the word: Strong
- Use ONLY one of the formats below.

FORMAT A (some matches, some lacks):
Strong <matched JD skills> skills but lacks <missing JD skills> experience.

FORMAT B (no JD skills match):
Strong candidate but lacks <all JD skills> experience.

FORMAT C (all JD skills match):
Strong alignment with all required skills.

CONSTRAINTS:
- Maximum 22 words.
- No bullet points.
- No headings.
- No explanations.
- No markdown.
- No role names.
- Only one final period.

FINAL ENFORCEMENT:
If any text outside the formats appears, discard it and output ONLY the corrected sentence.
[/INST]
""";



    var response = await _client.PostAsJsonAsync("/api/generate",
        new
        {
            model = "llama3",
            prompt = prompt,
            stream = false,
            options = new
            {
                // temperature = 0.0,
                // num_predict = 15,
                // num_ctx = 512
                num_predict = 50, 
                temperature = 0.1,  
                num_ctx = 2048
            }
        });

    response.EnsureSuccessStatusCode();

    var rawJson = await response.Content.ReadAsStringAsync();
    var result = JsonSerializer.Deserialize<OllamaGenerateResponse>(rawJson);

    return result?.Response;
}

}
