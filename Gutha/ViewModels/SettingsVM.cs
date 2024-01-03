using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Controls; // Required for Command
using Microsoft.Maui.Storage; // For Preferences
using System.Diagnostics; // For Debug
using System.Windows.Input;

namespace Gutha.ViewModels
{
    public class SettingsViewModel : INotifyPropertyChanged
    {
        private string _apiKey;
        private string _selectedVoice;
        private bool _isHdAudio;
        public List<string> Voices { get; } = new List<string> { "alloy", "echo", "fable", "onyx", "nova", "shimmer" };

        public string ApiKey
        {
            get => _apiKey;
            set
            {
                if (_apiKey != value)
                {
                    _apiKey = value;
                    OnPropertyChanged();
                }
            }
        }

      

        public bool IsHdAudio
        {
            get => _isHdAudio;
            set
            {
                if (_isHdAudio != value)
                {
                    _isHdAudio = value;
                    OnPropertyChanged();
                }
            }
        }


        public string SelectedVoice
        {
            get => _selectedVoice;
            set
            {
                if (_selectedVoice != value)
                {
                    _selectedVoice = value;
                    OnPropertyChanged();
                    Debug.WriteLine($"Selected Voice: {_selectedVoice}");

                    int selectedIndex = Voices.IndexOf(_selectedVoice);
                    Debug.WriteLine($"Selected Index: {selectedIndex}");
                }
            }
        }
        public ICommand SaveSettingsCommand { get; }

        public SettingsViewModel()
        {
             SaveSettingsCommand = new Command(SaveSettings);
            LoadSettings();
        }

        private void SaveSettings()
        {
            Preferences.Set("ApiKey", _apiKey);
            Preferences.Set("IsHdAudio", _isHdAudio);
            Preferences.Set("SelectedVoice", _selectedVoice);

            Debug.WriteLine($"Saved API Key: {_apiKey}");
            Debug.WriteLine($"Saved HD Audio setting: {_isHdAudio}");
            Debug.WriteLine($"Saved Selected Voice: {_selectedVoice}");
        }

        private void LoadSettings()
        {
            _apiKey = Preferences.Get("ApiKey", string.Empty);
            _isHdAudio = Preferences.Get("IsHdAudio", false);
            _selectedVoice = Preferences.Get("SelectedVoice", string.Empty);

            Debug.WriteLine($"Loaded API Key: {_apiKey}");
            Debug.WriteLine($"Loaded HD Audio setting: {_isHdAudio}");
            Debug.WriteLine($"Loaded Selected Voice: {_selectedVoice}");
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
