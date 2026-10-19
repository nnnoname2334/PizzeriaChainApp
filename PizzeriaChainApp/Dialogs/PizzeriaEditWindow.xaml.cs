using System;
using System.Windows;
using System.Windows.Input;
using PizzeriaChainApp.Data;
using PizzeriaChainApp.Helpers;

namespace PizzeriaChainApp.Dialogs
{
    public partial class PizzeriaEditWindow : Window
    {
        private readonly int? _id;

        // item == null: добавление новой пиццерии, иначе редактирование
        public PizzeriaEditWindow(pizzeria item = null)
        {
            InitializeComponent();

            _id = item?.id;
            Title = item == null ? "Новая пиццерия" : "Редактирование пиццерии № " + item.id;
            TextTitle.Text = Title;

            if (item != null)
            {
                TextBoxName.Text = item.name;
                TextBoxCity.Text = item.city;
                TextBoxAddress.Text = item.address;
                TextBoxPhone.Text = item.phone;
            }
        }

        private void TextBoxName_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !ValidationHelper.IsTitleChars(e.Text);
        }

        private void TextBoxCity_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !ValidationHelper.IsFullNameChars(e.Text);   // буквы, пробел, дефис
        }

        private void TextBoxAddress_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !ValidationHelper.IsAddressChars(e.Text);
        }

        private void TextBoxPhone_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !ValidationHelper.IsPhoneChars(e.Text);
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            string error = ValidationHelper.ValidatePizzeriaName(TextBoxName.Text)
                        ?? ValidationHelper.ValidateCity(TextBoxCity.Text)
                        ?? ValidationHelper.ValidateAddress(TextBoxAddress.Text)
                        ?? ValidationHelper.ValidateRequiredPhone(TextBoxPhone.Text);
            if (error != null)
            {
                MessageHelper.Warning(error);
                return;
            }

            try
            {
                PizzeriaService.Save(_id, TextBoxName.Text, TextBoxAddress.Text,
                                     TextBoxCity.Text, TextBoxPhone.Text);
                DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageHelper.Error("Не удалось сохранить пиццерию: " + ex.Message);
            }
        }
    }
}