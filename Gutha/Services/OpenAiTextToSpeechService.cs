using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using static Microsoft.Maui.ApplicationModel.Permissions;

namespace Gutha.Services
{
    public class OpenAiTextToSpeechService
    {
        private readonly HttpClient _httpClient;
        private const string ApiBaseUrl = "https://api.openai.com/v1/audio/speech";
        private readonly string _apiKey; // Store your API key securely

        public OpenAiTextToSpeechService(string apiKey)
        {
            _apiKey = apiKey;
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
        }

        public async Task<byte[]> ConvertTextToSpeechAsync(string text, string voice, bool isHd, double speed)
        {
            var payload = new
            {
                model = isHd ? "tts-1-hd" : "tts-1",
                input = text,
                voice = voice,
                speed = speed,
                response_format = "mp3"
            };

            var payloadString = JsonConvert.SerializeObject(payload);
            System.Diagnostics.Debug.WriteLine($"Request payload: {payloadString}");

            var content = new StringContent(payloadString, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(ApiBaseUrl, content);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsByteArrayAsync();
            }

            throw new Exception($"API request failed: {response.ReasonPhrase}");
        }
    }
}
