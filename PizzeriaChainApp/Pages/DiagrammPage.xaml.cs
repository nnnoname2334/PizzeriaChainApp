using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms.DataVisualization.Charting;
using System.Windows.Input;
using Microsoft.Win32;
using PizzeriaChainApp.Data;
using PizzeriaChainApp.Helpers;

namespace PizzeriaChainApp.Pages
{
    public partial class DiagrammPage : Page
    {
        public class ChartTypeItem
        {
            public string Name { get; set; }
            public SeriesChartType Type { get; set; }
        }

        private List<OrderRow> _orders = new List<OrderRow>();
        private List<pizzeria> _pizzerias = new List<pizzeria>();

        public DiagrammPage()
        {
            InitializeComponent();

            // Область построения и набор данных
            ChartOrders.BackColor = System.Drawing.Color.White;
            ChartOrders.ChartAreas.Add(new ChartArea("Main"));
            ChartOrders.Series.Add(new Series("Заказы")
            {
                IsValueShownAsLabel = true,
                LabelFormat = "N2"
            });
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                _orders = OrderService.GetOrders();
                _pizzerias = PizzeriaService.GetAll();
                ComboCustomer.ItemsSource = CustomerService.GetAll();
            }
            catch (Exception ex)
            {
                MessageHelper.Error("Не удалось загрузить данные: " + ex.Message);
                return;
            }

            // Только типы, подходящие для одного ряда данных
            ComboChartType.ItemsSource = new List<ChartTypeItem>
            {
                new ChartTypeItem { Name = "Столбчатая", Type = SeriesChartType.Column },
                new ChartTypeItem { Name = "Линейчатая", Type = SeriesChartType.Bar },
                new ChartTypeItem { Name = "График", Type = SeriesChartType.Line },
                new ChartTypeItem { Name = "Сглаженный график", Type = SeriesChartType.Spline },
                new ChartTypeItem { Name = "С областями", Type = SeriesChartType.Area },
                new ChartTypeItem { Name = "Точечная", Type = SeriesChartType.Point },
                new ChartTypeItem { Name = "Круговая", Type = SeriesChartType.Pie },
                new ChartTypeItem { Name = "Кольцевая", Type = SeriesChartType.Doughnut }
            };

            ComboChartType.SelectedIndex = 0;
            ComboCustomer.SelectedIndex = ComboCustomer.Items.Count > 0 ? 0 : -1;
        }

        // Перерисовка диаграммы при выборе клиента или типа диаграммы
        private void UpdateChart(object sender, SelectionChangedEventArgs e)
        {
            var selectedCustomer = ComboCustomer.SelectedItem as customer;
            var selectedType = ComboChartType.SelectedItem as ChartTypeItem;
            if (selectedCustomer == null || selectedType == null)
                return;

            Series series = ChartOrders.Series.First();
            series.ChartType = selectedType.Type;
            series.Points.Clear();

            bool isPie = selectedType.Type == SeriesChartType.Pie
                      || selectedType.Type == SeriesChartType.Doughnut;
            series.Color = isPie
                ? System.Drawing.Color.Empty
                : System.Drawing.ColorTranslator.FromHtml("#0198AF");

            foreach (pizzeria p in _pizzerias)
            {
                decimal sum = ReportService.ActiveSum(_orders.Where(o =>
                    o.CustomerId == selectedCustomer.id && o.PizzeriaId == p.id));

                if (isPie && sum == 0)
                    continue;   // нулевые доли на круговой диаграмме не нужны

                series.Points.AddXY(p.city + ", " + p.address, sum);
            }

            ChartOrders.Titles.Clear();
            ChartOrders.Titles.Add("Заказы клиента «" + selectedCustomer.full_name
                                   + "» по пиццериям, руб. (без отменённых)");
        }

        // ---------- Экспорт ----------
        private List<CustomerReport> LoadReports()
        {
            try
            {
                List<CustomerReport> reports = ReportService.Build();
                if (reports.Count == 0)
                {
                    MessageHelper.Warning("Нет заказов для экспорта.");
                    return null;
                }
                return reports;
            }
            catch (Exception ex)
            {
                MessageHelper.Error("Не удалось загрузить данные для экспорта: " + ex.Message);
                return null;
            }
        }

        private void ButtonExportWord_Click(object sender, RoutedEventArgs e)
        {
            List<CustomerReport> reports = LoadReports();
            if (reports == null)
                return;

            var dialog = new SaveFileDialog
            {
                Title = "Сохранить отчёт Word",
                Filter = "Документ Word (*.docx)|*.docx",
                FileName = "Заказы_клиентов_" + DateTime.Now.ToString("yyyyMMdd")
            };
            if (dialog.ShowDialog() != true)
                return;

            string docxPath = dialog.FileName;
            string pdfPath = Path.ChangeExtension(docxPath, ".pdf");

            RunExport(() => WordExporter.Export(reports, _pizzerias, docxPath, pdfPath),
                "Отчёт сохранён:\n" + docxPath + "\n" + pdfPath);
        }

        private void ButtonExportExcel_Click(object sender, RoutedEventArgs e)
        {
            List<CustomerReport> reports = LoadReports();
            if (reports == null)
                return;

            var dialog = new SaveFileDialog
            {
                Title = "Сохранить отчёт Excel",
                Filter = "Книга Excel (*.xlsx)|*.xlsx",
                FileName = "Заказы_клиентов_" + DateTime.Now.ToString("yyyyMMdd")
            };
            if (dialog.ShowDialog() != true)
                return;

            string path = dialog.FileName;
            RunExport(() => ExcelExporter.Export(reports, path), "Отчёт сохранён:\n" + path);
        }

        private void RunExport(Action export, string successText)
        {
            Mouse.OverrideCursor = Cursors.Wait;
            try
            {
                export();
                MessageHelper.Info(successText);
            }
            catch (Exception ex)
            {
                MessageHelper.Error("Не удалось выполнить экспорт. Убедитесь, что установлен "
                    + "Microsoft Office и файл не открыт в другой программе.\n\n" + ex.Message);
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }
    }
}