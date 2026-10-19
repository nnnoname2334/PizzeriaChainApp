using System.Windows;

namespace PizzeriaChainApp.Helpers
{
    public static class MessageHelper
    {
        private const string Caption = "Сеть пиццерий";

        public static void Info(string text)
        {
            MessageBox.Show(text, Caption, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public static void Warning(string text)
        {
            MessageBox.Show(text, Caption, MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        public static void Error(string text)
        {
            MessageBox.Show(text, Caption, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        // Запрос подтверждения: true, если пользователь нажал «Да»
        public static bool Confirm(string text)
        {
            return MessageBox.Show(text, Caption, MessageBoxButton.YesNo,
                MessageBoxImage.Question) == MessageBoxResult.Yes;
        }
    }
}