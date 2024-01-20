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
        #region Fields

        private IAudioPlayer _audioPlayer;
        private IAudioManager _audioManager;
        private FakeYouTextToSpeechService _ttsService;
        private double playbackSpeed = 1.0;
        private string _selectedModelToken;
        public Dictionary<string, string> ModelTokensDictionary { get; set; }
        private const string FakeYouModelToken = "TM:fmspb239ea3a";
        private List<string> _audioUrls = new List<string>();

        #endregion

        #region Constructor

        public FakeYou()
        {
            InitializeComponent();
            _audioManager = AudioManager.Current;
            _ttsService = new FakeYouTextToSpeechService();
            PopulateModelTokensDictionary();
            this.BindingContext = this;
        }

        #endregion

        #region Private Methods

        private void PopulateModelTokensDictionary()
        {
            ModelTokensDictionary = new Dictionary<string, string>
            {
                { "Yoda", "TM:fmspb239ea3a" },
                { "Donald Trump", "TM:djceg00wmcv5" },
                { "Donald Trump Angry", "TM:4v0ft4j72y2g" },
                { "Donkey (Shrek)", "TM:t2dnvad2n4g8" },
                { "Joe Biden","TM:6ctz239896cf"},
                { "C3P0 (Star Wars)", "TM:kz7xck6af35w" },
                { "Boba Fett", "TM:jpvktpbeq9p5" },
                { "James Earl Jones", "TM:785dsnba43hk" },
                { "Liam Neeson","TM:k158fr4f180j" },
                { "Clone Trooper","TM:n6bn57e75rh8" },
                {"Peggy Hill", "TM:zfy5xhbrgfw2"},
                {"Peter Griffin", "TM:ympn9keyq2n9"},
                {"Professor Farnsworth", "TM:64wbhzc3sr8x"},
                {"Mrs. Piggy", "TM:kmjexxq5hst8"},
                {"Kermit the Frog", "TM:ft86an38yf30"},
                {"Adam Sandler","TM:9jvnrmrer0ep" },
                {"Woody (Toy Story)","TM:808sy8zt6pts" },
                {"Buzz Lightyear","TM:y8k6b1ekk1t2" },
                {"Home Simpson","TM:2n4dwafb1t1r" },
                {"Mr Mackey (South Park)","TM:x8n248j4x42h" },
                { "My Garrison (South Park)","TM:y972m90w4g4a"},
                { "Zoidberg","TM:sa4znpqg717s"},
                { "Harry Potter","TM:nrwkc2acr6wa"},
                { "Mickey Mouse","TM:ar1cc7b9k3s8"}
            };
        }

        private async void OnSubmitClicked(object sender, EventArgs e)
        {
            try
            {
                _audioUrls.Clear();
                List<string> textSegments = SplitText(textInput.Text);

                foreach (var segment in textSegments)
                {
                    UpdateStatus("Creating TTS request for segment...");
                    var response = await _ttsService.CreateTtsRequestAsync(_selectedModelToken ?? FakeYouModelToken, segment);
                    var jobToken = ExtractJobToken(response);

                    Debug.WriteLine("TTS request created for segment.");

                    UpdateStatus("Processing TTS request for segment...");
                    string pollResponse;
                    do
                    {
                        await Task.Delay(1000);
                        pollResponse = await _ttsService.PollTtsRequestStatusAsync(jobToken);
                    }
                    while (!IsJobComplete(pollResponse));

                    Debug.WriteLine("TTS request processing complete for segment.");

                    UpdateStatus("Fetching audio URL for segment...");
                    var audioUrl = ExtractAudioUrl(pollResponse);
                    if (!string.IsNullOrEmpty(audioUrl))
                    {
                        _audioUrls.Add(audioUrl);
                        Debug.WriteLine("Audio URL added for segment.");
                    }
                }

                if (_audioUrls.Any())
                {
                    UpdateStatus("Playing audio segments...");
                    await PlayAudioSequence();
                    UpdateStatus("All audio segments played successfully.");
                }
                else
                {
                    UpdateStatus("No audio segments generated.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in OnSubmitClicked: {ex.Message}");
                UpdateStatus($"Error: {ex.Message}");
            }
        }

        private async Task PlayAudioFromUrl(string audioUrl)
        {
            var tcs = new TaskCompletionSource<bool>();
            EventHandler playbackEndedHandler = null;

            playbackEndedHandler = (sender, e) =>
            {
                _audioPlayer.PlaybackEnded -= playbackEndedHandler;
                tcs.SetResult(true);
            };

            using (var client = new HttpClient())
            {
                var audioStream = await client.GetStreamAsync(audioUrl);
                _audioPlayer = _audioManager.CreatePlayer(audioStream);

                if (_audioPlayer != null)
                {
                    _audioPlayer.PlaybackEnded += playbackEndedHandler;
                    _audioPlayer.Play();

                    // Wait for the playback to complete
                    await tcs.Task;
                }
            }
        }

        private async Task PlayAudioSequence()
        {
            foreach (var audioUrl in _audioUrls)
            {
                await PlayAudioFromUrl(audioUrl);
            }

            Debug.WriteLine("Finished playing all audio segments.");
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

        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.Count > 0)
            {
                var selectedItem = e.CurrentSelection[0]; // Get the selected item
                if (selectedItem is KeyValuePair<string, string> selectedPair)
                {
                    // Handle the selection
                    // For example, you can display the selected value or use it in some way
                    Console.WriteLine($"Selected: {selectedPair.Value}");
                }
            }
        }

        private List<string> SplitText(string inputText, int maxSegmentSize = 200)
        {
            List<string> segments = new List<string>();
            int startIndex = 0;

            Debug.WriteLine("Starting to split text.");

            while (startIndex < inputText.Length)
            {
                int segmentSize = maxSegmentSize;
                if (startIndex + segmentSize > inputText.Length)
                {
                    segmentSize = inputText.Length - startIndex; // Adjust to remaining length
                }
                else if (!char.IsWhiteSpace(inputText[startIndex + segmentSize - 1]))
                {
                    while (segmentSize > 0 && !char.IsWhiteSpace(inputText[startIndex + segmentSize - 1]))
                    {
                        segmentSize--;
                    }
                    if (segmentSize == 0)
                    {
                        segmentSize = maxSegmentSize; // Fallback to avoid zero length
                    }
                }

                string segment = inputText.Substring(startIndex, segmentSize);
                segments.Add(segment);

                Debug.WriteLine($"Segment added: {segment}");

                startIndex += segmentSize;
            }

            Debug.WriteLine("Finished splitting text.");

            return segments;
        }

        private void OnVoiceSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.Count > 0)
            {
                var selectedItem = e.CurrentSelection[0];
                if (selectedItem is KeyValuePair<string, string> selectedPair)
                {
                    _selectedModelToken = selectedPair.Value;
                    Console.WriteLine($"Selected Model Token: {_selectedModelToken}");
                }
            }
        }

        #endregion
    }
}