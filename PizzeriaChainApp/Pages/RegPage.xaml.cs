using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using PizzeriaChainApp.Helpers;

namespace PizzeriaChainApp.Pages
{
    public partial class RegPage : Page
    {
        public RegPage()
        {
            InitializeComponent();
        }

        // ---- Маски ввода ----
        private void TextBoxLogin_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !ValidationHelper.IsLoginChars(e.Text);
        }

        private void TextBoxFio_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !ValidationHelper.IsFullNameChars(e.Text);
        }

        private void TextBoxPhone_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !ValidationHelper.IsPhoneChars(e.Text);
        }

        private void PasswordBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !ValidationHelper.IsPasswordChars(e.Text);
        }

        private void ButtonRegister_Click(object sender, RoutedEventArgs e)
        {
            string login = TextBoxLogin.Text.Trim();
            string fio = TextBoxFio.Text.Trim();
            string phone = TextBoxPhone.Text.Trim();
            string pass = PasswordBoxFirst.Password;
            string pass2 = PasswordBoxSecond.Password;

            if (login == "" || fio == "" || pass == "" || pass2 == "")
            {
                MessageHelper.Warning("Заполните все обязательные поля!");
                return;
            }

            string error = ValidationHelper.ValidateLogin(login)
                        ?? ValidationHelper.ValidateFullName(fio)
                        ?? ValidationHelper.ValidatePhone(phone)
                        ?? ValidationHelper.ValidatePassword(pass);
            if (error != null)
            {
                MessageHelper.Warning(error);
                return;
            }

            if (pass != pass2)
            {
                MessageHelper.Warning("Пароли не совпадают!");
                return;
            }

            try
            {
                if (AuthService.LoginExists(login))
                {
                    MessageHelper.Warning("Пользователь с таким логином уже существует!");
                    return;
                }

                string role = (string)((ComboBoxItem)ComboRole.SelectedItem).Tag;
                AuthService.Register(login, pass, fio, phone, role);

                MessageHelper.Info("Пользователь успешно зарегистрирован!");
                TextBoxLogin.Clear();
                TextBoxFio.Clear();
                TextBoxPhone.Clear();
                PasswordBoxFirst.Clear();
                PasswordBoxSecond.Clear();
                ComboRole.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageHelper.Error("Не удалось сохранить пользователя: " + ex.Message);
            }
        }
    }
}