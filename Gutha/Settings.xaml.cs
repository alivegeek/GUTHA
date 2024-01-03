using Microsoft.Maui.Controls;
using Gutha.ViewModels;
using Microsoft.Maui.Graphics;

namespace Gutha
{
    public partial class Settings : ContentPage
    {
        private SettingsViewModel ViewModel => BindingContext as SettingsViewModel;

        public Settings()
        {
            InitializeComponent();
            BindingContext = new SettingsViewModel();
        }

        private Frame _lastSelectedFrame;

        private void OnVoiceOptionTapped(object sender, EventArgs e)
        {
            var frame = sender as Frame;
            if (frame != null)
            {
                // Reset the last selected frame
                if (_lastSelectedFrame != null)
                {
                    _lastSelectedFrame.BackgroundColor = Colors.Transparent;
                    _lastSelectedFrame.BorderColor = Colors.LightGray;
                }

                // Highlight the new selected frame
                frame.BackgroundColor = Colors.LightBlue;  // Or use your custom color
                frame.BorderColor = Colors.DarkBlue;      // Or use your custom color

                _lastSelectedFrame = frame;

                // Update your selected voice logic here
            }
        }


    }
}
