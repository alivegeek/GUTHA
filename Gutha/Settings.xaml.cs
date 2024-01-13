using Microsoft.Maui.Controls;
using Gutha.ViewModels;

namespace Gutha
{
    public partial class Settings : ContentPage
    {
        public Settings()
        {
            InitializeComponent();
            BindingContext = new SettingsViewModel();
        }
    }
}
