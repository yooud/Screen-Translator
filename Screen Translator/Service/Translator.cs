using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Screen_Translator.Models;

namespace Screen_Translator.Service;

public static class Translator
{
    private const string Url = "http://localhost:8080";

    public static async Task<string[]> GetLangauges()
    {
        using HttpClient client = new();
        var response = await client.PostAsync($"{Url}/api/languages", null!);
        var responseContent = await response.Content.ReadAsStringAsync();
        var languages = JsonSerializer.Deserialize<string[]>(responseContent);
        return languages!;
    }
    
    public static async Task<string[]> GetTessdata()
    {
        using HttpClient client = new();
        var response = await client.PostAsync($"{Url}/api/tessdata", null!);
        var responseContent = await response.Content.ReadAsStringAsync();
        var tessdata = JsonSerializer.Deserialize<string[]>(responseContent);
        return tessdata!;
    }

    public static async Task<string> Translate(string text, string dest, string src)
    {
        if (string.IsNullOrWhiteSpace(text)) return null!;
        
        var data = new { text, dest, src };
        
        using HttpClient client = new();
        var json = JsonSerializer.Serialize(data);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        var response = await client.PostAsync($"{Url}/api/translate", content);
        var responseContent = await response.Content.ReadAsStringAsync();
        var responseJson = JsonSerializer.Deserialize<Dictionary<string, string>>(responseContent);

        return responseJson?.TryGetValue("text", out var translatedText) ?? false ? translatedText : null!;
    }

    public static async Task Download(Language language)
    {
        using HttpClient client = new();
        var response = await client.PostAsync($"{Url}/api/tessdata/{language.Code}", null!);
        if (response.IsSuccessStatusCode)
        {
            await using Stream contentStream = await response.Content.ReadAsStreamAsync(), 
                fileStream = new FileStream(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tessdata", $"{language.Code}.traineddata"), FileMode.Create, FileAccess.Write);
            await contentStream.CopyToAsync(fileStream);
            language.IsDownloaded = true;
        }
    }
}