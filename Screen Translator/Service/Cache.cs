using System;
using System.IO;
using System.Runtime.Caching;
using System.Text.Json;
using System.Threading.Tasks;
using Screen_Translator.Models;

namespace Screen_Translator.Service;

public class Cache
{
    private readonly MemoryCache _cache = MemoryCache.Default;
    private readonly string _cacheFilePath = Path.Combine(Directory.GetCurrentDirectory(), "cache");
    private Settings? _settings;
    public event Action? UpdatedLanguages;
    public Settings? Settings
    {
        get
        {
            Save();
            return _settings;
        }
        private set => _settings = value;
    }

    public void Save() => File.WriteAllText(_cacheFilePath, JsonSerializer.Serialize(_settings));

    public void Load()
    {
        if (_cache.Get("settings") is not Models.Settings)
            if (File.Exists(_cacheFilePath))
            {
                _settings = JsonSerializer.Deserialize<Settings>(File.ReadAllText(_cacheFilePath))!;
                _cache.Set("settings", _settings, new CacheItemPolicy());
            }
            else
            {
                var task = Task.Run(() => // TODO Catch network error
                {
                    var languages = Translator.GetLangauges().GetAwaiter().GetResult();
                    var tessdata = Translator.GetTessdata().GetAwaiter().GetResult();
                    Settings = new Settings(languages, tessdata, languages[0], languages[1]);
                    _cache.Set("settings", Settings, new CacheItemPolicy());
                });
                task.Wait();
            }
    }

    public void UpdateDownloadedLanguages() => UpdatedLanguages?.Invoke();
}