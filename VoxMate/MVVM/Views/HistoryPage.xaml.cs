using VoxMate.MVVM.ViewModels;

namespace VoxMate.MVVM.Views;

public partial class HistoryPage : ContentPage
{
    private readonly AssistantViewModel _vm;

    public HistoryPage(AssistantViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = _vm;
    }

    private async void OnBackClicked(object? sender, EventArgs e)
    {
        try
        {
            await Navigation.PopAsync();
        }
        catch
        {
            await Application.Current.MainPage.Navigation.PopAsync();
        }
    }

    private async void OnClearHistoryClicked(object? sender, EventArgs e)
    {
        if (BindingContext is AssistantViewModel vm)
        {
            var ok = await DisplayAlert("Limpiar historial", "¿Borrar todo el historial?", "Sí", "No");
            if (ok)
            {
                vm.History.Clear();
            }
        }
    }
}