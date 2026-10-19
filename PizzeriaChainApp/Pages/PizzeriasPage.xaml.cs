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
    public partial class PizzeriasPage : Page
    {
        private const string AllCities = "Все города";
        private List<pizzeria> _all;
        private bool _loading;

        public PizzeriasPage()
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
                _all = PizzeriaService.GetAll();
            }
            catch (Exception ex)
            {
                _all = new List<pizzeria>();
                MessageHelper.Error("Не удалось загрузить пиццерии: " + ex.Message);
            }

            // Список городов для фильтра строится из данных
            _loading = true;
            string selected = ComboCity.SelectedItem as string;
            var cities = new List<string> { AllCities };
            cities.AddRange(_all.Select(p => p.city).Distinct().OrderBy(c => c));
            ComboCity.ItemsSource = cities;
            ComboCity.SelectedItem = (selected != null && cities.Contains(selected)) ? selected : AllCities;
            _loading = false;

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
            if (_all == null || _loading)
                return;

            IEnumerable<pizzeria> query = _all;

            string s = TextBoxSearch.Text.Trim();
            if (s != "")
                query = query.Where(p => Has(p.name, s) || Has(p.city, s)
                                      || Has(p.address, s) || Has(p.phone, s));

            string city = ComboCity.SelectedItem as string;
            if (city != null && city != AllCities)
                query = query.Where(p => p.city == city);

            switch (ComboSort.SelectedIndex)
            {
                case 1: query = query.OrderBy(p => p.name).ThenBy(p => p.city); break;
                case 2: query = query.OrderBy(p => p.address); break;
                default: query = query.OrderBy(p => p.city).ThenBy(p => p.address); break;
            }

            List<pizzeria> list = query.ToList();
            DataGridItems.ItemsSource = list;

            TextSummary.Text = list.Count == 0
                ? (_all.Count == 0 ? "Пиццерий пока нет." : "Пиццерии не найдены. Измените условия поиска или фильтры.")
                : "Найдено пиццерий: " + list.Count;
        }

        private void ButtonClear_Click(object sender, RoutedEventArgs e)
        {
            TextBoxSearch.Clear();
            ComboSort.SelectedIndex = 0;
            ComboCity.SelectedIndex = 0;
        }

        private void DataGridItems_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            bool selected = DataGridItems.SelectedItem != null;
            ButtonEdit.IsEnabled = selected;
            ButtonDelete.IsEnabled = selected;
        }

        private void ButtonAdd_Click(object sender, RoutedEventArgs e)
        {
            OpenEditor(null);
        }

        private void ButtonEdit_Click(object sender, RoutedEventArgs e)
        {
            var item = DataGridItems.SelectedItem as pizzeria;
            if (item != null)
                OpenEditor(item);
        }

        private void OpenEditor(pizzeria item)
        {
            try
            {
                var window = new PizzeriaEditWindow(item) { Owner = Window.GetWindow(this) };
                if (window.ShowDialog() == true)
                    LoadData();
            }
            catch (Exception ex)
            {
                MessageHelper.Error("Не удалось открыть форму пиццерии: " + ex.Message);
            }
        }

        private void ButtonDelete_Click(object sender, RoutedEventArgs e)
        {
            var item = DataGridItems.SelectedItem as pizzeria;
            if (item == null)
                return;

            try
            {
                int orders = PizzeriaService.CountOrders(item.id);
                string text = "Удалить пиццерию «" + item.name + "», " + item.city + ", " + item.address + "?";
                if (orders > 0)
                    text += "\n\nВместе с ней будут удалены все её заказы: " + orders + ".";
                text += "\n\nДействие нельзя отменить.";

                if (!MessageHelper.Confirm(text))
                    return;

                PizzeriaService.Delete(item.id);
                LoadData();
                MessageHelper.Info("Пиццерия удалена.");
            }
            catch (Exception ex)
            {
                MessageHelper.Error("Не удалось удалить пиццерию: " + ex.Message);
            }
        }
    }
}