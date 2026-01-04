using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;

public class MatcherController : Controller
{
    private readonly HttpClient _httpClient;

    public MatcherController(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("ResumeMatcherApi");
    }

    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Match(IFormFile resume, string jobDescription)
    {
        try
        {
            var form = new MultipartFormDataContent();

            var fileContent = new StreamContent(resume.OpenReadStream());
            fileContent.Headers.ContentType =
                new MediaTypeHeaderValue("application/pdf");

            form.Add(fileContent, "resume", resume.FileName);
            form.Add(new StringContent(jobDescription), "jobDescription");

            var response = await _httpClient.PostAsync("/api/match", form);

            if (!response.IsSuccessStatusCode)
            {
              var err = await response.Content.ReadAsStringAsync();
              TempData["ErrorMessage"] = "API Error: " + err;
              return View("Index");
            }
            var result = await response.Content.ReadAsStringAsync();

            //ViewBag.Result = result;
            //return View("Index"); 
            var matchResult = System.Text.Json.JsonSerializer.Deserialize<MatchResultViewModel>(result,new System.Text.Json.JsonSerializerOptions{PropertyNameCaseInsensitive = true});
            return View("Index", matchResult);

        }
        catch (Exception ex)
        {
            // ViewBag.Result = "Error: " + ex.Message;
            // return View("Index");
            TempData["ErrorMessage"] = "Could not connect to the AI service. Is Ollama running?";
            return View("Index");
        }
    }
}
