namespace Miko.Plugins.Anime;

using System.Net.Http.Json;

public struct AnimeSearchResult
{
    public int Anilist { get; set; }
    public string Filename { get; set; }
    public int? Episode { get; set; }
    public double From { get; set; }
    public double To { get; set; }
    public double Similarity { get; set; }
    public string Video { get; set; }
    public string Image { get; set; }
}

public struct AnimeSearchContent
{
    public long FrameCount { get; set; }
    public string Error { get; set; }
    public List<AnimeSearchResult> Result { get; set; }
}

public class AnimeSearchEngine
{
    private readonly HttpClient _httpClient = new() { BaseAddress = new("https://api.trace.moe/") };

    public AnimeSearchEngine(string apikey = "")
    {
        if (!string.IsNullOrEmpty(apikey))
            _httpClient.DefaultRequestHeaders.Add("x-trace-key", apikey);
    }

    public async Task<AnimeSearchContent> Search(string imageUrl)
    {
        return await _httpClient.GetFromJsonAsync<AnimeSearchContent>($"search?url={imageUrl}"); 
    }

    public async Task<string> GetMeAsync()
    {
        var response = await _httpClient.GetAsync("me");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }
}
