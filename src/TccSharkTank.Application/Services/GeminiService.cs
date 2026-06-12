using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using TccSharkTank.Application.Common;

namespace TccSharkTank.Application.Services;

public interface IGeminiService
{
    Task<List<long>> FiltrarIdeiasComIA(string buscaUsuario, string contextoIdeias, CancellationToken ct);
}

public sealed class GeminiService : IGeminiService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    public GeminiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _apiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY") ?? string.Empty;
    }

    public async Task<List<long>> FiltrarIdeiasComIA(string buscaUsuario, string contextoIdeias, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(_apiKey))
        {
            throw new AppException("GEMINI_API_KEY não configurada.", 503);
        }

        var prompt = $@"Você é um assistente inteligente de busca de um sistema estilo Shark Tank.
Abaixo estão as ideias (startups) cadastradas no formato [ID - Nome - Descrição]:
{contextoIdeias}

O investidor pesquisou por: '{buscaUsuario}'.
Retorne APENAS os IDs numéricos das ideias que fazem sentido para essa busca, separados por vírgula. Não escreva mais nada além dos números.";

        var payload = new { contents = new[] { new { parts = new[] { new { text = prompt } } } } };
        var jsonContent = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
        var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={_apiKey}";

        var response = await _httpClient.PostAsync(url, jsonContent, ct);
        if (!response.IsSuccessStatusCode)
        {
            throw new AppException("Falha ao consultar serviço de IA.", 503);
        }

        var jsonResponse = await response.Content.ReadAsStringAsync(ct);
        using var doc = JsonDocument.Parse(jsonResponse);
        var textoRetorno = string.Empty;
        if (doc.RootElement.TryGetProperty("candidates", out var candidates) &&
            candidates.ValueKind == JsonValueKind.Array &&
            candidates.GetArrayLength() > 0)
        {
            var candidate0 = candidates[0];
            if (candidate0.TryGetProperty("content", out var content) &&
                content.TryGetProperty("parts", out var parts) &&
                parts.ValueKind == JsonValueKind.Array &&
                parts.GetArrayLength() > 0)
            {
                var part0 = parts[0];
                if (part0.TryGetProperty("text", out var text) && text.ValueKind == JsonValueKind.String)
                {
                    textoRetorno = text.GetString() ?? string.Empty;
                }
            }
        }

        var idsEncontrados = new List<long>();
        var matches = Regex.Matches(textoRetorno, @"\d+");
        foreach (Match match in matches)
        {
            if (long.TryParse(match.Value, out long id))
            {
                idsEncontrados.Add(id);
            }
        }

        return idsEncontrados;
    }
}
