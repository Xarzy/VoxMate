using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Media;
using Microsoft.Extensions.Logging;
using VoxMate.Services;
using VoxMate.MVVM.Services;
using VoxMate.MVVM.ViewModels;
using VoxMate.MVVM.Views;

namespace VoxMate
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .UseMauiCommunityToolkitMediaElement()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
            builder.Logging.AddDebug();
#endif

            // --- CONFIGURACIÓN DE SERVICIOS (INYECCIÓN DE DEPENDENCIAS) ---
            builder.Services.AddSingleton<ISpeechToText>(SpeechToText.Default);
            builder.Services.AddSingleton<VoiceService>();

            builder.Services.AddTransient<AssistantPage>();
            builder.Services.AddTransient<AssistantViewModel>();
            builder.Services.AddTransient<MainPage>();
            builder.Services.AddTransient<MainViewModel>();
            builder.Services.AddTransient<HistoryPage>();

            var app = builder.Build();

            // Inicializar tema una vez existe Application.Current
            ThemeService.Initialize();

            return app;
        }
    }
}