using UglyToad.PdfPig;
using System.Text;

public class ResumeParserService
{
    public string ExtractTextFromPdf(Stream stream)
    {
        using var pdf = PdfDocument.Open(stream);
        var text = new StringBuilder();

        foreach (var page in pdf.GetPages())
            text.Append(page.Text);

        return text.ToString();
    }
}
