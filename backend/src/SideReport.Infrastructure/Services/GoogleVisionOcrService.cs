using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SideReport.Application.Interfaces;

namespace SideReport.Infrastructure.Services;

/// <summary>
/// Google Cloud Vision API를 사용한 OCR 서비스
/// </summary>
public class GoogleVisionOcrService : IOcrService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly ILogger<GoogleVisionOcrService> _logger;

    private const string VisionApiUrl =
        "https://vision.googleapis.com/v1/images:annotate";

    public GoogleVisionOcrService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<GoogleVisionOcrService> logger)
    {
        _httpClient = httpClient;
        _apiKey = configuration["GoogleVision:ApiKey"]
            ?? throw new InvalidOperationException("GoogleVision:ApiKey가 설정되지 않았습니다.");
        _logger = logger;
    }

    public async Task<OcrTextResult> ExtractTextAsync(Stream imageStream, string mimeType)
    {
        try
        {
            // 이미지를 Base64로 인코딩
            using var ms = new MemoryStream();
            await imageStream.CopyToAsync(ms);
            var base64Image = Convert.ToBase64String(ms.ToArray());

            var requestBody = new
            {
                requests = new[]
                {
                    new
                    {
                        image = new { content = base64Image },
                        features = new[]
                        {
                            new { type = "TEXT_DETECTION", maxResults = 1 }
                        },
                        imageContext = new
                        {
                            languageHints = new[] { "ko", "en" }
                        }
                    }
                }
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var url = $"{VisionApiUrl}?key={_apiKey}";

            var response = await _httpClient.PostAsync(url, content);
            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(responseJson);

            var textAnnotations = doc.RootElement
                .GetProperty("responses")[0]
                .GetProperty("textAnnotations");

            if (textAnnotations.GetArrayLength() == 0)
                return new OcrTextResult(string.Empty, true);

            var rawText = textAnnotations[0].GetProperty("description").GetString() ?? string.Empty;
            return new OcrTextResult(rawText, true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Google Vision OCR 처리 중 오류 발생");
            return new OcrTextResult(string.Empty, false, ex.Message);
        }
    }
}
