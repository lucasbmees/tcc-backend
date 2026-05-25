using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace TccSharkTank.Application.Services;

public interface IGeminiService
{
    Task<List<long>> FiltrarIdeiasComIA(string buscaUsuario, string contextoIdeias, CancellationToken ct);
}

public sealed class GeminiService : IGeminiService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey = "COLOCA A CHAVE AQUI"; 

    public GeminiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<long>> FiltrarIdeiasComIA(string buscaUsuario, string contextoIdeias, CancellationToken ct)
    {
        var prompt = $@"Você é um assistente inteligente de busca de um sistema estilo Shark Tank.
Abaixo estão as ideias (startups) cadastradas no formato [ID - Nome - Descrição]:
{contextoIdeias}

O investidor pesquisou por: '{buscaUsuario}'.
Retorne APENAS os IDs numéricos das ideias que fazem sentido para essa busca, separados por vírgula. Não escreva mais nada além dos números.";

        var payload = new { contents = new[] { new { parts = new[] { new { text = prompt } } } } };
        var jsonContent = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
        var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={_apiKey}";

        var response = await _httpClient.PostAsync(url, jsonContent, ct);
        
        // --- NOVO BLOCO DE DEBUG ---
        if (!response.IsSuccessStatusCode) 
        {
            var erroDoGoogle = await response.Content.ReadAsStringAsync(ct);
            Console.WriteLine("\n=== ERRO DA IA (GOOGLE) ===");
            Console.WriteLine($"Status Code: {response.StatusCode}");
            Console.WriteLine($"Detalhe: {erroDoGoogle}");
            Console.WriteLine("===========================\n");
            return new List<long>();
        }
        // ---------------------------

        var jsonResponse = await response.Content.ReadAsStringAsync(ct);
        using var doc = JsonDocument.Parse(jsonResponse);
        
        var textoRetorno = doc.RootElement
            .GetProperty("candidates")[0]
            .GetProperty("content")
            .GetProperty("parts")[0]
            .GetProperty("text")
            .GetString() ?? "";

        // ==========================================
        // ADICIONE ESTAS 5 LINHAS AQUI PARA DEBUG:
        Console.WriteLine("\n=== DEBUG DA IA ===");
        Console.WriteLine($"O QUE O C# MANDOU: {contextoIdeias}");
        Console.WriteLine($"O QUE A IA RESPONDEU: {textoRetorno}");
        Console.WriteLine("===================\n");
        // ==========================================

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