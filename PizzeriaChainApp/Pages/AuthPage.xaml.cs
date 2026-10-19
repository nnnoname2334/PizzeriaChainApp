using System.Windows;
using System.Windows.Controls;
using PizzeriaChainApp.Data;
using PizzeriaChainApp.Helpers;

namespace PizzeriaChainApp.Pages
{
    public partial class AuthPage : Page
    {
        private const int MaxAttempts = 3;
        // static: счётчик не сбрасывается, если уйти на другую страницу и вернуться
        private static int _failedAttempts = 0;
        private string _captchaValue;

        public AuthPage()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            // После выхода из аккаунта нельзя вернуться «Назад» на закрытые страницы
            while (NavigationService != null && NavigationService.CanGoBack)
                NavigationService.RemoveBackEntry();

            AppData.CurrentUser = null;

            if (_failedAttempts >= MaxAttempts)
                ShowCaptcha();
        }

        private void TextBoxLogin_PreviewTextInput(object sender,
            System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = !ValidationHelper.IsLoginChars(e.Text);
        }

        private void ButtonEnter_Click(object sender, RoutedEventArgs e)
        {
            string login = TextBoxLogin.Text.Trim();
            string password = PasswordBox.Password;

            if (login == "" || password == "")
            {
                MessageHelper.Warning("Введите логин и пароль.");
                return;
            }

            customer user = AuthService.Authenticate(login, password);

            if (user == null)
            {
                _failedAttempts++;
                PasswordBox.Clear();

                if (_failedAttempts >= MaxAttempts)
                {
                    MessageHelper.Error("Слишком много неверных попыток. Пройдите проверку капчей.");
                    ShowCaptcha();
                }
                else
                {
                    MessageHelper.Error("Неверный логин или пароль. Попыток до капчи: "
                                        + (MaxAttempts - _failedAttempts) + ".");
                }
                return;
            }

            _failedAttempts = 0;
            AppData.CurrentUser = user;

            switch (user.role)
            {
                case "Admin":
                    NavigationService?.Navigate(new AdminPage());
                    break;
                case "User":
                    NavigationService?.Navigate(new UserPage());
                    break;
                default:
                    MessageHelper.Error("Для учётной записи не задана корректная роль.");
                    break;
            }
        }

        private void ButtonReg_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new RegPage());
        }

        private void ButtonChangePassword_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new ChangePassPage());
        }

        // ---------- Капча ----------
        private void ShowCaptcha()
        {
            LoginPanel.Visibility = Visibility.Collapsed;
            CaptchaPanel.Visibility = Visibility.Visible;
            ButtonEnter.IsDefault = false;
            ButtonSubmitCaptcha.IsDefault = true;
            RefreshCaptcha();
        }

        private void HideCaptcha()
        {
            CaptchaPanel.Visibility = Visibility.Collapsed;
            LoginPanel.Visibility = Visibility.Visible;
            ButtonSubmitCaptcha.IsDefault = false;
            ButtonEnter.IsDefault = true;
            TextBoxLogin.Clear();
            PasswordBox.Clear();
        }

        private void RefreshCaptcha()
        {
            _captchaValue = CaptchaHelper.Generate();
            CaptchaText.Text = _captchaValue;
            CaptchaInput.Clear();
        }

        private void ButtonRefreshCaptcha_Click(object sender, RoutedEventArgs e)
        {
            RefreshCaptcha();
        }

        private void ButtonSubmitCaptcha_Click(object sender, RoutedEventArgs e)
        {
            if (!CaptchaHelper.Check(CaptchaInput.Text, _captchaValue))
            {
                MessageHelper.Error("Капча введена неверно. Код обновлён.");
                RefreshCaptcha();
                return;
            }

            _failedAttempts = 0;
            MessageHelper.Info("Проверка пройдена. Можете продолжить вход.");
            HideCaptcha();
        }
    }
}