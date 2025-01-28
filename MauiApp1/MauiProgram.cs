using MauiApp1.Services; // Importing the services used in the application
using Microsoft.Extensions.Logging;
using CommunityToolkit.Maui;// For logging purposes

namespace MauiApp1
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();

            // Configure the app with essential settings
            builder
               .UseMauiApp<App>()
               .UseMauiCommunityToolkit()
               .ConfigureFonts(fonts =>
               {
                   fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                   fonts.AddFont("materialdesignicons-webfont.ttf", "MaterialIcons");
               });

            // Add Blazor WebView support
            builder.Services.AddMauiBlazorWebView();
            // Registering services
            builder.Services.AddScoped<MainService>(); // Scoped: Instance per request
            builder.Services.AddSingleton<StateService>(); // Singleton: Single instance for the lifetime of the app

#if DEBUG
            // Add developer tools and debug logging in debug mode
            builder.Services.AddBlazorWebViewDeveloperTools();
            builder.Logging.AddDebug();
#endif

            // Build and return the configured app
            return builder.Build();
        }
    }
}
