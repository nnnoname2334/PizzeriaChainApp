using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using PizzeriaChainApp.Data;
using PizzeriaChainApp.Helpers;

namespace PizzeriaChainApp.Dialogs
{
    public partial class OrderEditWindow : Window
    {
        private const string DateFormat = "dd.MM.yyyy HH:mm";
        private readonly int? _orderId;

        // order == null: добавление нового заказа, иначе редактирование
        public OrderEditWindow(OrderRow order = null)
        {
            InitializeComponent();

            _orderId = order?.Id;
            Title = order == null ? "Новый заказ" : "Редактирование заказа № " + order.Id;
            TextTitle.Text = Title;

            ComboCustomer.ItemsSource = OrderService.GetCustomerLookup();
            ComboPizzeria.ItemsSource = OrderService.GetPizzeriaLookup();

            if (order == null)
            {
                ComboType.SelectedIndex = 0;
                ComboStatus.SelectedIndex = 0;
                TextBoxDate.Text = DateTime.Now.ToString(DateFormat);
            }
            else
            {
                ComboCustomer.SelectedValue = order.CustomerId;
                ComboPizzeria.SelectedValue = order.PizzeriaId;
                TextBoxDate.Text = order.OrderDate.ToString(DateFormat);
                SelectItem(ComboType, order.OrderType);
                SelectItem(ComboStatus, order.Status);
                TextBoxAmount.Text = order.TotalAmount.ToString("0.##", new CultureInfo("ru-RU"));
            }
        }

        private static void SelectItem(ComboBox combo, string text)
        {
            combo.SelectedItem = combo.Items.OfType<ComboBoxItem>()
                .FirstOrDefault(i => (string)i.Content == text);
        }

        // ---- Маски ввода ----
        private void TextBoxDate_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !ValidationHelper.IsDateTimeChars(e.Text);
        }

        private void TextBoxAmount_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !ValidationHelper.IsAmountChars(e.Text);
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            if (ComboCustomer.SelectedValue == null || ComboPizzeria.SelectedValue == null)
            {
                MessageHelper.Warning("Выберите клиента и пиццерию.");
                return;
            }

            DateTime date = DateTime.MinValue;
            decimal total = 0;
            string error = ValidationHelper.ValidateOrderDate(TextBoxDate.Text, out date)
                        ?? ValidationHelper.ValidateAmount(TextBoxAmount.Text, out total);
            if (error != null)
            {
                MessageHelper.Warning(error);
                return;
            }

            try
            {
                OrderService.SaveOrder(
                    _orderId,
                    (int)ComboCustomer.SelectedValue,
                    (int)ComboPizzeria.SelectedValue,
                    date,
                    (string)((ComboBoxItem)ComboType.SelectedItem).Content,
                    (string)((ComboBoxItem)ComboStatus.SelectedItem).Content,
                    total);

                DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageHelper.Error("Не удалось сохранить заказ: " + ex.Message);
            }
        }
    }
}