var builder = WebApplication.CreateBuilder(args);

// API only
builder.Services.AddControllers();

builder.Services.AddHttpClient("OllamaClient", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Ollama:BaseUrl"]);
    client.Timeout = TimeSpan.FromMinutes(5);
});


// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Services
builder.Services.AddSingleton<ResumeParserService>();
builder.Services.AddSingleton<OllamaEmbeddingService>();
builder.Services.AddSingleton<SimilarityService>();
builder.Services.AddSingleton<ExplanationService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();

// API endpoints ONLY
app.MapControllers();

app.Run();
