using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using PizzeriaChainApp.Data;
using PizzeriaChainApp.Helpers;

namespace PizzeriaChainApp.Pages
{
    public partial class UserPage : Page
    {
        private List<OrderRow> _allOrders;

        public UserPage()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            // Нельзя вернуться «Назад» на страницу входа без выхода из аккаунта
            while (NavigationService != null && NavigationService.CanGoBack)
                NavigationService.RemoveBackEntry();

            try
            {
                // Пользователь видит только свои заказы
                _allOrders = OrderService.GetOrders(AppData.CurrentUser.id);
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
            // Обработчики срабатывают ещё при построении страницы, когда данных нет
            if (_allOrders == null)
                return;

            List<OrderRow> list = OrderService.Filter(
                _allOrders,
                TextBoxSearch.Text,
                GetFilterValue(ComboType, "Все типы"),
                GetFilterValue(ComboStatus, "Все статусы"),
                ComboSort.SelectedIndex);

            DataGridOrders.ItemsSource = list;

            if (list.Count == 0)
            {
                TextSummary.Text = _allOrders.Count == 0
                    ? "У вас пока нет заказов."
                    : "Заказы не найдены. Измените условия поиска или фильтры.";
            }
            else
            {
                decimal sum = list.Where(o => o.Status != "Отменён").Sum(o => o.TotalAmount);
                TextSummary.Text = "Найдено заказов: " + list.Count
                                 + ". Сумма без учёта отменённых: " + sum.ToString("N2") + " ₽";
            }
        }

        // Выбранное значение фильтра или null, если выбран пункт «Все ...»
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

        private void ButtonLogout_Click(object sender, RoutedEventArgs e)
        {
            if (!MessageHelper.Confirm("Вы действительно хотите выйти из аккаунта?"))
                return;

            AppData.CurrentUser = null;
            NavigationService?.Navigate(new AuthPage());
        }
    }
}