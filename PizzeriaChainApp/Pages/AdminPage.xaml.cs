using System.Windows;
using System.Windows.Controls;
using PizzeriaChainApp.Data;
using PizzeriaChainApp.Helpers;

namespace PizzeriaChainApp.Pages
{
    public partial class AdminPage : Page
    {
        public AdminPage()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            // Нельзя вернуться «Назад» на страницу входа без выхода из аккаунта
            while (NavigationService != null && NavigationService.CanGoBack)
                NavigationService.RemoveBackEntry();

            TextWelcome.Text = "Здравствуйте, " + AppData.CurrentUser?.full_name + "!";
        }

        private void ButtonLogout_Click(object sender, RoutedEventArgs e)
        {
            if (!MessageHelper.Confirm("Вы действительно хотите выйти из аккаунта?"))
                return;

            AppData.CurrentUser = null;
            NavigationService?.Navigate(new AuthPage());
        }
    }
}