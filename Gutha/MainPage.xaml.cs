﻿using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using Gutha.Services;
using System.Diagnostics;
using System.Threading.Tasks;
using Plugin.Maui.Audio; 
using System.IO; 
using AVKit;

namespace Gutha
{
    public partial class MainPage : ContentPage
    {
        private IAudioPlayer _audioPlayer;
        private IAudioManager _audioManager;
        private double playbackSpeed = 1.0; 

        public MainPage()
        {
            InitializeComponent();
            _audioManager = AudioManager.Current;
        }


        protected override void OnAppearing()
        {
            base.OnAppearing();
        }

       

        private void OnTextInputChanged(object sender, TextChangedEventArgs e)
        {
            UpdateEstimatedCost();
        }

        private async void OnSubmitClicked(object sender, EventArgs e)
        {
            try
            {
                var directApiKey = Preferences.Get("ApiKey", "Default Value");

                // Stop and dispose of the existing audio player if it exists
                if (_audioPlayer != null)
                {
                    _audioPlayer.Stop();
                    _audioPlayer.Dispose();
                }

                // Retrieve the selected voice from preferences
                var selectedVoice = Preferences.Get("SelectedVoice", "alloy"); // Default to "alloy" if not set
                                                                               // Assume playbackSpeed is already set by another part of your UI, like a speed selection button

                var ttsService = new OpenAiTextToSpeechService(directApiKey);
                var audioBytes = await ttsService.ConvertTextToSpeechAsync(textInput.Text, selectedVoice, AppSettings.IsHdAudio, playbackSpeed);

                // Play the new audio
                await PlayAudio(audioBytes);
                UpdateStatus("Audio generated successfully.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in OnSubmitClicked: {ex.Message}");
                UpdateStatus($"Error: {ex.Message}");
            }
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

        private async Task PlayAudio(byte[] audioBytes)
        {
            using (var stream = new MemoryStream(audioBytes))
            {
                // Create a new audio player for the new audio stream
                _audioPlayer = _audioManager.CreatePlayer(stream);

                // Check if the player was successfully created
                if (_audioPlayer == null)
                {
                    Debug.WriteLine("Failed to create the audio player.");
                    return;
                }

                _audioPlayer.Play();
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

        private void OnSpeedChanged(object sender, EventArgs e)
        {
            if (sender is Button button)
            {
                playbackSpeed = Convert.ToDouble(button.Text.Replace("x", ""));
            }
        }


    }
}
