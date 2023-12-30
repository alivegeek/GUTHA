using Microsoft.Maui.Storage;

namespace Gutha
{
    public static class AppSettings
    {
        private const string ApiKeyKey = "ApiKey";

        public static string ApiKey
        {
            get => Preferences.Get(ApiKeyKey, string.Empty);
            set => Preferences.Set(ApiKeyKey, value);
        }

        public static bool IsHdAudio { get; set; } = false; // Existing code
    }
}
