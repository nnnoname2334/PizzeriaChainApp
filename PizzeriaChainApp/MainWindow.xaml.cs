using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Windows.Threading;
using PizzeriaChainApp.Helpers;

namespace PizzeriaChainApp
{
    public partial class MainWindow : Window
    {
        private const string AppTitle = "Сеть пиццерий";

        public MainWindow()
        {
            InitializeComponent();
            MainFrame.Navigate(new Pages.AuthPage());
        }

        // Часы в правом нижнем углу
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            DateTimeNow.Text = DateTime.Now.ToString();
            var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            timer.Tick += (o, t) => DateTimeNow.Text = DateTime.Now.ToString();
            timer.Start();
        }

        // Выход с подтверждением
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (!MessageHelper.Confirm("Вы действительно хотите выйти?"))
                e.Cancel = true;
        }

        private void ButtonBack_Click(object sender, RoutedEventArgs e)
        {
            if (MainFrame.CanGoBack)
                MainFrame.GoBack();
        }

        private void ButtonTheme_Click(object sender, RoutedEventArgs e)
        {
            ThemeManager.Toggle();
        }

        // Динамический заголовок: берётся из свойства Title текущей страницы
        private void MainFrame_Navigated(object sender, NavigationEventArgs e)
        {

            string title = (MainFrame.Content as Page)?.Title ?? "";
            PageTitleText.Text = title;
            Title = string.IsNullOrEmpty(title) ? AppTitle : AppTitle + " — " + title;
        }
    }
}