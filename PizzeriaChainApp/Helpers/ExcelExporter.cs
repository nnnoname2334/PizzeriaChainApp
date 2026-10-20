using System;
using System.Collections.Generic;
using System.Linq;
using PizzeriaChainApp.Data;
using Excel = Microsoft.Office.Interop.Excel;

namespace PizzeriaChainApp.Helpers
{
    public static class ExcelExporter
    {
        public static void Export(List<CustomerReport> reports, string path)
        {
            var application = new Excel.Application();
            try
            {
                application.SheetsInNewWorkbook = reports.Count;
                Excel.Workbook workbook = application.Workbooks.Add(Type.Missing);

                var usedNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                decimal grandTotal = 0;

                for (int i = 0; i < reports.Count; i++)
                {
                    CustomerReport report = reports[i];
                    int row = 1;

                    Excel.Worksheet worksheet = (Excel.Worksheet)application.Worksheets.Item[i + 1];
                    worksheet.Name = SafeSheetName(report.CustomerName, usedNames);

                    // Заголовки столбцов
                    string[] headers = { "Дата заказа", "№ заказа", "Тип заказа", "Статус", "Сумма" };
                    for (int c = 0; c < headers.Length; c++)
                        worksheet.Cells[row, c + 1] = headers[c];

                    Excel.Range headerRange = worksheet.Range[worksheet.Cells[row, 1], worksheet.Cells[row, 5]];
                    headerRange.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
                    headerRange.Font.Bold = true;
                    row++;

                    // Заказы, сгруппированные по пиццериям
                    var groups = report.Orders
                        .GroupBy(o => o.PizzeriaId)
                        .OrderBy(g => g.First().City)
                        .ThenBy(g => g.First().Address);

                    foreach (var group in groups)
                    {
                        OrderRow first = group.First();

                        Excel.Range groupRange = worksheet.Range[worksheet.Cells[row, 1], worksheet.Cells[row, 5]];
                        groupRange.Merge();
                        groupRange.Value2 = first.PizzeriaName + ", " + first.FullAddress;
                        groupRange.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
                        groupRange.Font.Italic = true;
                        row++;

                        int firstDataRow = row;
                        foreach (OrderRow order in group.OrderBy(o => o.OrderDate))
                        {
                            worksheet.Cells[row, 1] = order.OrderDate.ToString("dd.MM.yyyy HH:mm");
                            worksheet.Cells[row, 2] = order.Id;
                            worksheet.Cells[row, 3] = order.OrderType;
                            worksheet.Cells[row, 4] = order.Status;
                            worksheet.Cells[row, 5] = (double)order.TotalAmount;
                            ((Excel.Range)worksheet.Cells[row, 5]).NumberFormat = "0.00";
                            row++;
                        }
                        int lastDataRow = row - 1;

                        // ИТОГО по пиццерии: формула, отменённые заказы не суммируются
                        Excel.Range sumRange = worksheet.Range[worksheet.Cells[row, 1], worksheet.Cells[row, 4]];
                        sumRange.Merge();
                        sumRange.Value2 = "ИТОГО (без отменённых):";
                        sumRange.HorizontalAlignment = Excel.XlHAlign.xlHAlignRight;

                        Excel.Range totalCell = (Excel.Range)worksheet.Cells[row, 5];
                        totalCell.Formula = "=SUMIF(D" + firstDataRow + ":D" + lastDataRow
                                          + ",\"<>Отменён\",E" + firstDataRow + ":E" + lastDataRow + ")";
                        totalCell.NumberFormat = "0.00";
                        sumRange.Font.Bold = true;
                        totalCell.Font.Bold = true;
                        row++;
                    }

                    // Границы таблицы и ширина столбцов
                    Excel.Range borders = worksheet.Range[worksheet.Cells[1, 1], worksheet.Cells[row - 1, 5]];
                    borders.Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
                    worksheet.Columns.AutoFit();

                    grandTotal += ReportService.ActiveSum(report.Orders);
                }

                // Итоговый лист
                Excel.Worksheet summarySheet = (Excel.Worksheet)workbook.Worksheets.Add(
                    Type.Missing, workbook.Worksheets[workbook.Worksheets.Count],
                    Type.Missing, Type.Missing);
                summarySheet.Name = "Общий итог";
                summarySheet.Cells[1, 1] = "Общий итог:";
                summarySheet.Cells[1, 2] = (double)grandTotal;
                ((Excel.Range)summarySheet.Cells[1, 2]).NumberFormat = "0.00";

                Excel.Range summaryRange = summarySheet.Range[summarySheet.Cells[1, 1], summarySheet.Cells[1, 2]];
                summaryRange.Font.Color = Excel.XlRgbColor.rgbRed;
                summaryRange.Font.Bold = true;
                summarySheet.Columns.AutoFit();

                application.DisplayAlerts = false;
                workbook.SaveAs(path);
                application.DisplayAlerts = true;

                application.Visible = true;   // показываем готовую книгу
            }
            catch
            {
                try { application.Quit(); } catch { }
                throw;
            }
        }

        // Имя листа: до 31 символа, без запрещённых символов, уникальное
        private static string SafeSheetName(string name, HashSet<string> used)
        {
            foreach (char ch in new[] { ':', '\\', '/', '?', '*', '[', ']' })
                name = name.Replace(ch, ' ');

            name = name.Trim();
            if (name.Length > 31)
                name = name.Substring(0, 31);
            if (name == "")
                name = "Лист";

            string unique = name;
            int n = 2;
            while (!used.Add(unique))
            {
                string suffix = " (" + n + ")";
                unique = name.Substring(0, Math.Min(name.Length, 31 - suffix.Length)) + suffix;
                n++;
            }
            return unique;
        }
    }
}