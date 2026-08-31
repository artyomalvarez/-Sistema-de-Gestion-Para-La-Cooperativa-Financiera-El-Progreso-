using System.Text.Json;
using Cooperativa_El_Progreso.console.Models;

namespace Cooperativa_El_Progreso.console.Services;

public class TrmService : ITrmService
{
    private readonly HttpClient _httpClient;
    private const string TrmApiUrl = "https://www.datos.gov.co/resource/32sa-8pi3.json?$order=vigenciadesde%20DESC&$limit=1";

    public TrmService(HttpClient? httpClient = null)
    {
        _httpClient = httpClient ?? new HttpClient();
    }

    public async Task<TrmResponseDto?> GetCurrentTrmAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync(TrmApiUrl);

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var jsonStream = await response.Content.ReadAsStreamAsync();
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var trmList = await JsonSerializer.DeserializeAsync<List<TrmResponseDto>>(jsonStream, options);

            return trmList?.FirstOrDefault();
        }
        catch (Exception ex)
        {
            // Business Rule: If the API fails (network error, timeout, DNS issue, deserialization error),
            // the application MUST NOT crash. Log/inspect if necessary and return null gracefully.
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine($"[Warning] Failed to fetch TRM data: {ex.Message}");
            Console.ResetColor();
            return null;
        }
    }
}
