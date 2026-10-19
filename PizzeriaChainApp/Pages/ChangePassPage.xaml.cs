using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using PizzeriaChainApp.Helpers;

namespace PizzeriaChainApp.Pages
{
    public partial class ChangePassPage : Page
    {
        public ChangePassPage()
        {
            InitializeComponent();
        }

        private void TextBoxLogin_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !ValidationHelper.IsLoginChars(e.Text);
        }

        private void PasswordBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !ValidationHelper.IsPasswordChars(e.Text);
        }

        private void ButtonChange_Click(object sender, RoutedEventArgs e)
        {
            string login = TextBoxLogin.Text.Trim();
            string oldPass = PasswordBoxOld.Password;
            string newPass = PasswordBoxNew.Password;
            string newPass2 = PasswordBoxNew2.Password;

            if (login == "" || oldPass == "" || newPass == "" || newPass2 == "")
            {
                MessageHelper.Warning("Заполните все поля!");
                return;
            }

            string error = ValidationHelper.ValidatePassword(newPass);
            if (error != null)
            {
                MessageHelper.Warning(error);
                return;
            }

            if (newPass != newPass2)
            {
                MessageHelper.Warning("Новые пароли не совпадают!");
                return;
            }

            if (newPass == oldPass)
            {
                MessageHelper.Warning("Новый пароль должен отличаться от текущего.");
                return;
            }

            try
            {
                if (!AuthService.ChangePassword(login, oldPass, newPass))
                {
                    MessageHelper.Error("Неверный логин или текущий пароль.");
                    return;
                }

                MessageHelper.Info("Пароль успешно изменён.");
                NavigationService?.GoBack();
            }
            catch (Exception ex)
            {
                MessageHelper.Error("Не удалось изменить пароль: " + ex.Message);
            }
        }
    }
}