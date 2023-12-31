using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices;

namespace Gutha
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            if (DeviceInfo.Idiom == DeviceIdiom.Desktop || DeviceInfo.Idiom == DeviceIdiom.Tablet)
            {
                FlyoutBehavior = FlyoutBehavior.Locked;
            }
            else
            {
                FlyoutBehavior = FlyoutBehavior.Flyout;
            }

            // Additional setup can go here
        }

        // You can add methods for navigation or other logic specific to your app shell
    }
}
