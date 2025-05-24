using System.Text.Json;

namespace EtkinlikSistemiAPI.Services
{
    // Hava durumu ile ilgili işlemleri gerçekleştiren servis sınıfı
    public class WeatherService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey = "seninapıkey";

        public WeatherService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string> GetWeatherConditionAsync(string cityName)
        {
            var url = $"https://api.openweathermap.org/data/2.5/weather?q={cityName}&appid={_apiKey}&lang=tr&units=metric";

            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
                return "Bilinmiyor";

            var json = await response.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(json);
            var description = doc.RootElement.GetProperty("weather")[0].GetProperty("description").GetString();

            return description;
        }

        // 🔽 1. Tarihli hava durumu (forecast) verisi
        public async Task<string> GetWeatherDescriptionAsync(string city, DateTime date)
        {
            var url = $"https://api.openweathermap.org/data/2.5/forecast?q={city}&appid={_apiKey}&units=metric&lang=tr";

            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
                return "Bilinmiyor";

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var list = doc.RootElement.GetProperty("list");

            JsonElement? closest = null;
            TimeSpan smallestDiff = TimeSpan.MaxValue;

            foreach (var item in list.EnumerateArray())
            {
                var dtTxt = item.GetProperty("dt_txt").GetString();
                var itemDate = DateTime.Parse(dtTxt);

                var diff = (itemDate - date).Duration();
                if (diff < smallestDiff)
                {
                    smallestDiff = diff;
                    closest = item;
                }
            }

            if (closest != null)
            {
                return closest.Value.GetProperty("weather")[0].GetProperty("description").GetString();
            }

            return "Bilinmiyor";
        }

        // 🔽 2. Açıklamaya göre planlama uygun mu
        public bool IsEventPlanSuitable(string weatherDescription)
        {
            var unsuitableConditions = new[] { "yağmur", "fırtına", "kar", "şiddetli", "sağanak", "dolu" };

            foreach (var condition in unsuitableConditions)
            {
                if (weatherDescription.ToLower().Contains(condition))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
