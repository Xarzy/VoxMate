using Microsoft.Extensions.DependencyInjection;
using VoxMate.MVVM.ViewModels;

namespace VoxMate.MVVM.Views;

public partial class MainPage : ContentPage
{
    private readonly IServiceProvider _services;

    public MainPage(
        MainViewModel viewModel,
        IServiceProvider services)
    {
        InitializeComponent();
        BindingContext = viewModel;
        _services = services;
    }

    private async void OnSwipe(object sender, SwipedEventArgs e)
    {
        // Resolver la página desde DI
        var page = _services.GetRequiredService<AssistantPage>();
        await Navigation.PushAsync(page);
    }

    // Manejador añadido para el botón "Historial" definido en XAML
    private async void OnOpenHistoryClicked(object sender, EventArgs e)
    {
        var page = _services.GetRequiredService<HistoryPage>();
        await Navigation.PushAsync(page);
    }
}