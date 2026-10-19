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
    public partial class AdminPage : Page
    {
        private List<OrderRow> _allOrders;

        public AdminPage()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            // Нельзя вернуться «Назад» на страницу входа без выхода из аккаунта
            while (NavigationService != null && NavigationService.CanGoBack)
                NavigationService.RemoveBackEntry();

            LoadOrders();
        }

        private void LoadOrders()
        {
            try
            {
                _allOrders = OrderService.GetOrders();   // администратор видит все заказы
            }
            catch (Exception ex)
            {
                _allOrders = new List<OrderRow>();
                MessageHelper.Error("Не удалось загрузить заказы: " + ex.Message);
            }
            ApplyFilters();
        }

        private void Filter_Changed(object sender, EventArgs e)
        {
            ApplyFilters();
        }

        private void ApplyFilters()
        {
            if (_allOrders == null)
                return;

            List<OrderRow> list = OrderService.Filter(
                _allOrders,
                TextBoxSearch.Text,
                GetFilterValue(ComboType, "Все типы"),
                GetFilterValue(ComboStatus, "Все статусы"),
                ComboSort.SelectedIndex,
                searchByCustomer: true);

            DataGridOrders.ItemsSource = list;

            if (list.Count == 0)
            {
                TextSummary.Text = _allOrders.Count == 0
                    ? "Заказов пока нет."
                    : "Заказы не найдены. Измените условия поиска или фильтры.";
            }
            else
            {
                decimal sum = list.Where(o => o.Status != "Отменён").Sum(o => o.TotalAmount);
                TextSummary.Text = "Найдено заказов: " + list.Count
                                 + ". Сумма без учёта отменённых: " + sum.ToString("N2") + " ₽";
            }
        }

        private static string GetFilterValue(ComboBox combo, string allText)
        {
            string value = (combo.SelectedItem as ComboBoxItem)?.Content as string;
            return (value == null || value == allText) ? null : value;
        }

        private void ButtonClear_Click(object sender, RoutedEventArgs e)
        {
            TextBoxSearch.Clear();
            ComboSort.SelectedIndex = 0;
            ComboType.SelectedIndex = 0;
            ComboStatus.SelectedIndex = 0;
        }

        // Изменить и Удалить доступны только при выбранной строке
        private void DataGridOrders_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            bool selected = DataGridOrders.SelectedItem != null;
            ButtonEdit.IsEnabled = selected;
            ButtonDelete.IsEnabled = selected;
        }

        private void ButtonAdd_Click(object sender, RoutedEventArgs e)
        {
            OpenEditor(null);
        }

        private void ButtonEdit_Click(object sender, RoutedEventArgs e)
        {
            var row = DataGridOrders.SelectedItem as OrderRow;
            if (row != null)
                OpenEditor(row);
        }

        private void OpenEditor(OrderRow row)
        {
            try
            {
                var window = new OrderEditWindow(row) { Owner = Window.GetWindow(this) };
                if (window.ShowDialog() == true)
                    LoadOrders();
            }
            catch (Exception ex)
            {
                MessageHelper.Error("Не удалось открыть форму заказа: " + ex.Message);
            }
        }

        private void ButtonDelete_Click(object sender, RoutedEventArgs e)
        {
            var row = DataGridOrders.SelectedItem as OrderRow;
            if (row == null)
                return;

            if (!MessageHelper.Confirm("Удалить заказ № " + row.Id + " от "
                    + row.OrderDate.ToString("dd.MM.yyyy HH:mm") + "? Действие нельзя отменить."))
                return;

            try
            {
                OrderService.DeleteOrder(row.Id);
                LoadOrders();
                MessageHelper.Info("Заказ удалён.");
            }
            catch (Exception ex)
            {
                MessageHelper.Error("Не удалось удалить заказ: " + ex.Message);
            }
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