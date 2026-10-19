using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using PizzeriaChainApp.Data;
using PizzeriaChainApp.Dialogs;
using PizzeriaChainApp.Helpers;

namespace PizzeriaChainApp.Pages
{
    public partial class CustomersPage : Page
    {
        private List<customer> _all;

        public CustomersPage()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        private void LoadData()
        {
            try
            {
                _all = CustomerService.GetAll();
            }
            catch (Exception ex)
            {
                _all = new List<customer>();
                MessageHelper.Error("Не удалось загрузить клиентов: " + ex.Message);
            }
            ApplyFilters();
        }

        private void Filter_Changed(object sender, EventArgs e)
        {
            ApplyFilters();
        }

        private static bool Has(string value, string text)
        {
            return value != null &&
                   value.IndexOf(text, StringComparison.CurrentCultureIgnoreCase) >= 0;
        }

        private void ApplyFilters()
        {
            if (_all == null)
                return;

            IEnumerable<customer> query = _all;

            string s = TextBoxSearch.Text.Trim();
            if (s != "")
                query = query.Where(c => Has(c.login, s) || Has(c.full_name, s)
                                      || Has(c.phone, s) || Has(c.email, s));

            string role = (ComboRole.SelectedItem as ComboBoxItem)?.Tag as string;
            if (!string.IsNullOrEmpty(role))
                query = query.Where(c => c.role == role);

            switch (ComboSort.SelectedIndex)
            {
                case 1: query = query.OrderByDescending(c => c.full_name); break;
                case 2: query = query.OrderBy(c => c.bonus_points); break;
                case 3: query = query.OrderByDescending(c => c.bonus_points); break;
                default: query = query.OrderBy(c => c.full_name); break;
            }

            List<customer> list = query.ToList();
            DataGridItems.ItemsSource = list;

            TextSummary.Text = list.Count == 0
                ? (_all.Count == 0 ? "Клиентов пока нет." : "Клиенты не найдены. Измените условия поиска или фильтры.")
                : "Найдено клиентов: " + list.Count;
        }

        private void ButtonClear_Click(object sender, RoutedEventArgs e)
        {
            TextBoxSearch.Clear();
            ComboSort.SelectedIndex = 0;
            ComboRole.SelectedIndex = 0;
        }

        private void DataGridItems_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            bool selected = DataGridItems.SelectedItem != null;
            ButtonEdit.IsEnabled = selected;
            ButtonDelete.IsEnabled = selected;
        }

        private void ButtonEdit_Click(object sender, RoutedEventArgs e)
        {
            var item = DataGridItems.SelectedItem as customer;
            if (item == null)
                return;

            try
            {
                var window = new CustomerEditWindow(item) { Owner = Window.GetWindow(this) };
                if (window.ShowDialog() == true)
                    LoadData();
            }
            catch (Exception ex)
            {
                MessageHelper.Error("Не удалось открыть форму клиента: " + ex.Message);
            }
        }

        private void ButtonDelete_Click(object sender, RoutedEventArgs e)
        {
            var item = DataGridItems.SelectedItem as customer;
            if (item == null)
                return;

            try
            {
                int orders = CustomerService.CountOrders(item.id);
                string text = "Удалить клиента «" + item.full_name + "» (" + item.login + ")?";
                if (orders > 0)
                    text += "\n\nВместе с ним будут удалены все его заказы: " + orders + ".";
                text += "\n\nДействие нельзя отменить.";

                if (!MessageHelper.Confirm(text))
                    return;

                CustomerService.Delete(item.id);
                LoadData();
                MessageHelper.Info("Клиент удалён.");
            }
            catch (Exception ex)
            {
                MessageHelper.Error(ex.Message);
            }
        }
    }
}