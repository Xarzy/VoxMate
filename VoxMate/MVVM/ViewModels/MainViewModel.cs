using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace VoxMate.MVVM.ViewModels
{
    public class MainViewModel
    {
        private readonly IServiceProvider _services;

        public ICommand StartCommand { get; }

        public MainViewModel(IServiceProvider services)
        {
            _services = services ?? throw new ArgumentNullException(nameof(services));

            StartCommand = new Command(async () =>
            {
                // Resolvemos la página desde DI (asegúrate de registrar AssistantPage en MauiProgram)
                var page = _services.GetService(typeof(VoxMate.MVVM.Views.AssistantPage)) as Page;

                // Usamos la navegación global (MainPage) para navegar
                await Application.Current.MainPage.Navigation.PushAsync(page);
            });
        }
    }
}
