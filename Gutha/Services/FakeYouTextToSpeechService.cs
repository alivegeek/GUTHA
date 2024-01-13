using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Gutha.Services
{
    public class FakeYouTextToSpeechService
    {
        private readonly HttpClient _httpClient;
        private const string ApiBaseUrl = "https://api.fakeyou.com";
        private readonly string _apiKey; // Optional API key

        public FakeYouTextToSpeechService(string apiKey = null)
        {
            _apiKey = apiKey;
            _httpClient = new HttpClient();

            if (!string.IsNullOrEmpty(_apiKey))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
            }
        }

        public async Task<string> CreateTtsRequestAsync(string modelToken, string text)
        {
            var payload = new
            {
                tts_model_token = modelToken,
                uuid_idempotency_token = Guid.NewGuid().ToString(),
                inference_text = text
            };

            var payloadString = JsonConvert.SerializeObject(payload);
            var content = new StringContent(payloadString, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{ApiBaseUrl}/tts/inference", content);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> PollTtsRequestStatusAsync(string jobToken)
        {
            var response = await _httpClient.GetAsync($"{ApiBaseUrl}/tts/job/{jobToken}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

        public string GetAudioFileUrl(string path)
        {
            return $"https://storage.googleapis.com/vocodes-public{path}";
        }
    }
}
