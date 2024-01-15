using Microsoft.Extensions.Logging;
using Plugin.Maui.Audio;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Controls.Hosting;
using System.Collections.ObjectModel;
using CommunityToolkit.Maui;  // Add this for Community Toolkit

namespace Gutha;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // Register the Community Toolkit
        builder.UseMauiCommunityToolkit();

        // Register the audio manager
        builder.Services.AddSingleton(AudioManager.Current);

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
