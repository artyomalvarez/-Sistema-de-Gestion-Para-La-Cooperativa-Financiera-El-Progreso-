using System.Text.Json.Serialization;

namespace Cooperativa_El_Progreso.console.Models;

public class TrmResponseDto
{
    [JsonPropertyName("valor")]
    public string Value { get; set; } = string.Empty;

    [JsonPropertyName("unidad")]
    public string Unit { get; set; } = string.Empty;

    [JsonPropertyName("vigenciadesde")]
    public DateTime ValidityFrom { get; set; }

    [JsonPropertyName("vigenciahasta")]
    public DateTime ValidityTo { get; set; }

    public decimal NumericValue => decimal.TryParse(Value, System.Globalization.CultureInfo.InvariantCulture, out var result) ? result : 0m;
}
