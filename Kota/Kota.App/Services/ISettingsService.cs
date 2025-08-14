using Newtonsoft.Json;
using System.IO;

namespace Kota.App.Services
{
    public interface ISettingsService
    {
        Task<T?> GetSettingAsync<T>(string key);
        Task SetSettingAsync<T>(string key, T value);
        Task<string?> GetDatabasePathAsync();
        Task SetDatabasePathAsync(string path);
    }

    public class SettingsService : ISettingsService
    {
        private readonly string _settingsPath;
        private Dictionary<string, object> _settings = new();

        public SettingsService()
        {
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var kotaFolder = Path.Combine(appDataPath, "Kota");
            Directory.CreateDirectory(kotaFolder);
            _settingsPath = Path.Combine(kotaFolder, "settings.json");

            LoadSettings();
        }

        public async Task<T?> GetSettingAsync<T>(string key)
        {
            if (_settings.TryGetValue(key, out var value))
            {
                if (value is T directValue)
                    return directValue;

                if (value is string json)
                    return JsonConvert.DeserializeObject<T>(json);
            }

            return default(T);
        }

        public async Task SetSettingAsync<T>(string key, T value)
        {
            _settings[key] = value!;
            await SaveSettingsAsync();
        }

        public async Task<string?> GetDatabasePathAsync()
        {
            return await GetSettingAsync<string>("DatabasePath");
        }

        public async Task SetDatabasePathAsync(string path)
        {
            await SetSettingAsync("DatabasePath", path);
        }

        private void LoadSettings()
        {
            try
            {
                if (File.Exists(_settingsPath))
                {
                    var json = File.ReadAllText(_settingsPath);
                    _settings = JsonConvert.DeserializeObject<Dictionary<string, object>>(json) ?? new();
                }
            }
            catch (Exception ex)
            {
                // Log error, start with empty settings
                _settings = new Dictionary<string, object>();
            }
        }

        private async Task SaveSettingsAsync()
        {
            try
            {
                var json = JsonConvert.SerializeObject(_settings, Formatting.Indented);
                await File.WriteAllTextAsync(_settingsPath, json);
            }
            catch (Exception ex)
            {
                // Log error
            }
        }
    }
}