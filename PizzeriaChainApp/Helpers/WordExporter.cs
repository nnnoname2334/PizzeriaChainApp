using System;
using System.Collections.Generic;
using System.Linq;
using PizzeriaChainApp.Data;
using Word = Microsoft.Office.Interop.Word;

namespace PizzeriaChainApp.Helpers
{
    public static class WordExporter
    {
        // Word (.docx) и копия в PDF рядом
        public static void Export(List<CustomerReport> reports, List<pizzeria> pizzerias,
                                  string docxPath, string pdfPath)
        {
            var application = new Word.Application();
            try
            {
                Word.Document document = BuildDocument(application, reports, pizzerias);
                document.SaveAs2(docxPath);
                document.ExportAsFixedFormat(pdfPath, Word.WdExportFormat.wdExportFormatPDF);
                application.Visible = true;
            }
            catch
            {
                try { application.Quit(Word.WdSaveOptions.wdDoNotSaveChanges); } catch { }
                throw;
            }
        }

        // Только PDF: Word работает скрыто и закрывается
        public static void ExportPdf(List<CustomerReport> reports, List<pizzeria> pizzerias,
                                     string pdfPath)
        {
            var application = new Word.Application();
            try
            {
                application.Visible = false;
                Word.Document document = BuildDocument(application, reports, pizzerias);
                document.ExportAsFixedFormat(pdfPath, Word.WdExportFormat.wdExportFormatPDF);
                document.Close(Word.WdSaveOptions.wdDoNotSaveChanges);
            }
            finally
            {
                try { application.Quit(Word.WdSaveOptions.wdDoNotSaveChanges); } catch { }
            }
        }

        private static Word.Document BuildDocument(Word.Application application,
            List<CustomerReport> reports, List<pizzeria> pizzerias)
        {
            Word.Document document = application.Documents.Add();

            for (int u = 0; u < reports.Count; u++)
            {
                CustomerReport report = reports[u];

                AddParagraph(document, report.CustomerName, Word.WdBuiltinStyle.wdStyleHeading1,
                    Word.WdParagraphAlignment.wdAlignParagraphCenter);
                AddParagraph(document, "", Word.WdBuiltinStyle.wdStyleNormal,
                    Word.WdParagraphAlignment.wdAlignParagraphLeft);

                Word.Paragraph tableParagraph = document.Paragraphs.Add();
                Word.Range tableRange = tableParagraph.Range;
                Word.Table table = document.Tables.Add(tableRange, pizzerias.Count + 1, 2);
                table.Range.set_Style(Word.WdBuiltinStyle.wdStyleNormal);
                table.Borders.InsideLineStyle = Word.WdLineStyle.wdLineStyleSingle;
                table.Borders.OutsideLineStyle = Word.WdLineStyle.wdLineStyleSingle;
                table.Range.Cells.VerticalAlignment =
                    Word.WdCellVerticalAlignment.wdCellAlignVerticalCenter;

                Word.Range cellRange = table.Cell(1, 1).Range;
                cellRange.Text = "Пиццерия";
                cellRange = table.Cell(1, 2).Range;
                cellRange.Text = "Сумма заказов (без отменённых)";

                table.Rows[1].Range.Font.Name = "Times New Roman";
                table.Rows[1].Range.Font.Size = 14;
                table.Rows[1].Range.Bold = 1;
                table.Rows[1].Range.ParagraphFormat.Alignment =
                    Word.WdParagraphAlignment.wdAlignParagraphCenter;

                for (int i = 0; i < pizzerias.Count; i++)
                {
                    pizzeria p = pizzerias[i];
                    decimal sum = ReportService.ActiveSum(
                        report.Orders.Where(o => o.PizzeriaId == p.id));

                    cellRange = table.Cell(i + 2, 1).Range;
                    cellRange.Text = p.name + ", " + p.city + ", " + p.address;
                    cellRange.Font.Name = "Times New Roman";
                    cellRange.Font.Size = 12;

                    cellRange = table.Cell(i + 2, 2).Range;
                    cellRange.Text = sum.ToString("N2") + " руб.";
                    cellRange.Font.Name = "Times New Roman";
                    cellRange.Font.Size = 12;
                }

                AddParagraph(document, "", Word.WdBuiltinStyle.wdStyleNormal,
                    Word.WdParagraphAlignment.wdAlignParagraphLeft);

                List<OrderRow> active = report.Orders
                    .Where(o => o.Status != ReportService.CancelledStatus).ToList();

                if (active.Count > 0)
                {
                    OrderRow max = active.OrderByDescending(o => o.TotalAmount).First();
                    OrderRow min = active.OrderBy(o => o.TotalAmount).First();

                    AddParagraph(document, "Самый дорогой заказ – № " + max.Id + ", "
                        + max.FullAddress + ", на " + max.TotalAmount.ToString("N2")
                        + " руб. от " + max.OrderDate.ToString("dd.MM.yyyy"),
                        Word.WdBuiltinStyle.wdStyleSubtitle,
                        Word.WdParagraphAlignment.wdAlignParagraphLeft,
                        Word.WdColor.wdColorDarkRed);

                    AddParagraph(document, "Самый дешёвый заказ – № " + min.Id + ", "
                        + min.FullAddress + ", на " + min.TotalAmount.ToString("N2")
                        + " руб. от " + min.OrderDate.ToString("dd.MM.yyyy"),
                        Word.WdBuiltinStyle.wdStyleSubtitle,
                        Word.WdParagraphAlignment.wdAlignParagraphLeft,
                        Word.WdColor.wdColorDarkGreen);
                }
                else
                {
                    AddParagraph(document, "Действующих заказов нет (все заказы отменены).",
                        Word.WdBuiltinStyle.wdStyleNormal,
                        Word.WdParagraphAlignment.wdAlignParagraphLeft);
                }

                if (u < reports.Count - 1)
                    document.Words.Last.InsertBreak(Word.WdBreakType.wdPageBreak);
            }

            foreach (Word.Section section in document.Sections)
            {
                Word.HeaderFooter footer =
                    section.Footers[Word.WdHeaderFooterIndex.wdHeaderFooterPrimary];
                footer.PageNumbers.Add(Word.WdPageNumberAlignment.wdAlignPageNumberCenter, true);

                Word.Range headerRange =
                    section.Headers[Word.WdHeaderFooterIndex.wdHeaderFooterPrimary].Range;
                headerRange.Text = "Отчёт сформирован: " + DateTime.Now.ToString("dd.MM.yyyy");
                headerRange.ParagraphFormat.Alignment =
                    Word.WdParagraphAlignment.wdAlignParagraphCenter;
                headerRange.Font.Size = 10;
            }

            return document;
        }

        private static void AddParagraph(Word.Document document, string text,
            Word.WdBuiltinStyle style, Word.WdParagraphAlignment alignment,
            Word.WdColor? color = null)
        {
            Word.Paragraph paragraph = document.Paragraphs.Add();
            Word.Range range = paragraph.Range;
            range.Text = text;
            paragraph.set_Style(style);
            range.ParagraphFormat.Alignment = alignment;
            if (color.HasValue)
                range.Font.Color = color.Value;
            range.InsertParagraphAfter();
        }
    }
}