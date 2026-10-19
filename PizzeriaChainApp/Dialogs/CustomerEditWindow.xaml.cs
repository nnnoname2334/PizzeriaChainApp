using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using PizzeriaChainApp.Data;
using PizzeriaChainApp.Helpers;

namespace PizzeriaChainApp.Dialogs
{
    public partial class CustomerEditWindow : Window
    {
        private readonly int _id;

        public CustomerEditWindow(customer item)
        {
            InitializeComponent();

            _id = item.id;
            Title = "Редактирование клиента № " + item.id;
            TextTitle.Text = Title;

            TextBoxLogin.Text = item.login;
            TextBoxFio.Text = item.full_name;
            TextBoxPhone.Text = item.phone;
            TextBoxEmail.Text = item.email;
            TextBoxBonus.Text = item.bonus_points.ToString();
            ComboRole.SelectedItem = ComboRole.Items.OfType<ComboBoxItem>()
                .FirstOrDefault(i => (string)i.Tag == item.role);
        }

        private void TextBoxFio_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !ValidationHelper.IsFullNameChars(e.Text);
        }

        private void TextBoxPhone_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !ValidationHelper.IsPhoneChars(e.Text);
        }

        private void TextBoxEmail_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !ValidationHelper.IsEmailChars(e.Text);
        }

        private void TextBoxBonus_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !ValidationHelper.IsDigitsOnly(e.Text);
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            int bonus = 0;
            string error = ValidationHelper.ValidateFullName(TextBoxFio.Text.Trim())
                        ?? ValidationHelper.ValidatePhone(TextBoxPhone.Text.Trim())
                        ?? ValidationHelper.ValidateEmail(TextBoxEmail.Text)
                        ?? ValidationHelper.ValidateBonus(TextBoxBonus.Text, out bonus);
            if (error != null)
            {
                MessageHelper.Warning(error);
                return;
            }

            try
            {
                string role = (string)((ComboBoxItem)ComboRole.SelectedItem).Tag;
                CustomerService.Update(_id, TextBoxFio.Text, TextBoxPhone.Text,
                                       TextBoxEmail.Text, bonus, role);
                DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageHelper.Error("Не удалось сохранить клиента: " + ex.Message);
            }
        }
    }
}