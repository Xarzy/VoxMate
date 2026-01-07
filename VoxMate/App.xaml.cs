using VoxMate.MVVM.Views;

namespace VoxMate
{
    public partial class App : Application
    {
        public App(MainPage mainPage)
        {
            InitializeComponent();
            MainPage = new NavigationPage(mainPage);
        }
    }
}