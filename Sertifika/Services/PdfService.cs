using System.Text.Json;
using System.Text.Json.Serialization;

namespace Sertifika.Services;

public class PdfService : IPdfService
{
    private readonly HttpClient _httpClient;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public PdfService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<byte[]> GenerateSingleAsync(PdfGenerateRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync("/generate", request, JsonOptions);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsByteArrayAsync();
    }

    public async Task<List<PdfBatchResult>> GenerateBatchAsync(PdfBatchRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync("/generate-batch", request, JsonOptions);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var results = new List<PdfBatchResult>();

        foreach (var item in doc.RootElement.GetProperty("results").EnumerateArray())
        {
            results.Add(new PdfBatchResult
            {
                Participant = item.GetProperty("participant").GetString() ?? "",
                File = item.TryGetProperty("file", out var f) ? f.GetString() : null,
                Error = item.TryGetProperty("error", out var e) ? e.GetString() : null,
                Success = item.GetProperty("success").GetBoolean()
            });
        }

        return results;
    }

    public async Task<byte[]> PreviewAsync(PdfGenerateRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync("/preview", request, JsonOptions);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsByteArrayAsync();
    }
}
