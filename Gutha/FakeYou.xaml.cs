using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using Gutha.Services;
using System.Diagnostics;
using System.Threading.Tasks;
using Plugin.Maui.Audio;
using System.IO;
using System.Net.Http;
using System;
using AVKit;

namespace Gutha
{
    public partial class FakeYou : ContentPage
    {
        private IAudioPlayer _audioPlayer;
        private IAudioManager _audioManager;
        private FakeYouTextToSpeechService _ttsService;
        private const string FakeYouModelToken = "TM:fmspb239ea3a"; // Thats Yoda <<
        private double playbackSpeed = 1.0;

        public FakeYou()
        {
            InitializeComponent();
            _audioManager = AudioManager.Current;
            _ttsService = new FakeYouTextToSpeechService(); // Initialize without API Key as per your requirement
        }

        private async void OnSubmitClicked(object sender, EventArgs e)
        {
            try
            {
                // Stop and dispose of the existing audio player if it exists
                if (_audioPlayer != null)
                {
                    _audioPlayer.Stop();
                    _audioPlayer.Dispose();
                }

                // Update status to reflect that TTS request is being created
                UpdateStatus("Creating TTS request...");

                // Create TTS request
                var response = await _ttsService.CreateTtsRequestAsync(FakeYouModelToken, textInput.Text);
                var jobToken = ExtractJobToken(response); // Implement this method to extract job token from response

                // Update status to reflect that TTS request is processing
                UpdateStatus("Processing TTS request...");

                // Poll for TTS request status
                string pollResponse;
                do
                {
                    await Task.Delay(1000); // Wait for 1 second before polling again
                    pollResponse = await _ttsService.PollTtsRequestStatusAsync(jobToken);
                }
                while (!IsJobComplete(pollResponse)); // Implement this method to check if job is complete

                // Update status to reflect that audio is being fetched
                UpdateStatus("Fetching audio...");

                var audioUrl = ExtractAudioUrl(pollResponse); // Implement this method to extract audio URL
                await PlayAudioFromUrl(audioUrl);

                UpdateStatus("Audio generated successfully.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in OnSubmitClicked: {ex.Message}");
                UpdateStatus($"Error: {ex.Message}");
            }
        }

        private async Task PlayAudioFromUrl(string audioUrl)
        {
            using (var client = new HttpClient())
            {
                var audioStream = await client.GetStreamAsync(audioUrl);
                _audioPlayer = _audioManager.CreatePlayer(audioStream);
                if (_audioPlayer != null)
                {
                    _audioPlayer.Play();
                }
            }
        }

        private void OnTextInputChanged(object sender, TextChangedEventArgs e)
        {
            UpdateEstimatedCost();
        }

        private void OnSpeedButtonClicked(object sender, EventArgs e)
        {
            if (sender is Button speedButton)
            {
                var speedText = speedButton.Text.Replace("x", ""); // Remove the 'x' from the text
                if (double.TryParse(speedText, out double speed))
                {
                    playbackSpeed = speed;
                }
            }
        }

        private void OnPlayPauseClicked(object sender, EventArgs e)
        {
            if (_audioPlayer == null)
            {
                Debug.WriteLine("Audio player not initialized.");
                return;
            }

            if (_audioPlayer.IsPlaying)
            {
                _audioPlayer.Pause();
            }
            else
            {
                _audioPlayer.Play();
            }
        }

        private void UpdateEstimatedCost()
        {
            int characterCount = textInput.Text?.Length ?? 0;
            bool isHdAudio = Preferences.Get("IsHdAudio", false);
            double costPerThousandCharacters = isHdAudio ? 0.030 : 0.015;
            double estimatedCost = (characterCount / 1000.0) * costPerThousandCharacters;
            estimatedCostLabel.Text = $"Estimated cost: ${estimatedCost:F3}";
        }

        private void UpdateStatus(string message)
        {
            statusLabel.Text = message;
        }

        private string ExtractJobToken(string response)
        {
            var jsonResponse = Newtonsoft.Json.Linq.JObject.Parse(response);
            return jsonResponse["inference_job_token"]?.ToString();
        }


        private bool IsJobComplete(string pollResponse)
        {
            var jsonResponse = Newtonsoft.Json.Linq.JObject.Parse(pollResponse);
            string status = jsonResponse["state"]?["status"]?.ToString();
            return status == "complete_success";
        }


        private string ExtractAudioUrl(string pollResponse)
        {
            var jsonResponse = Newtonsoft.Json.Linq.JObject.Parse(pollResponse);
            string path = jsonResponse["state"]?["maybe_public_bucket_wav_audio_path"]?.ToString();
            return path != null ? $"https://storage.googleapis.com/vocodes-public{path}" : null;
        }

    }
}
