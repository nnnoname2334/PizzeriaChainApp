using System.Linq;
using System.Text.RegularExpressions;

namespace PizzeriaChainApp.Helpers
{
    public static class ValidationHelper
    {
        // ---- Маски ввода: допустимы ли символы (вызываются из PreviewTextInput) ----
        public static bool IsLoginChars(string text)
        {
            return text.All(ch => (ch >= 'A' && ch <= 'Z') || (ch >= 'a' && ch <= 'z')
                               || (ch >= '0' && ch <= '9') || ch == '_');
        }

        public static bool IsFullNameChars(string text)
        {
            return Regex.IsMatch(text, @"^[A-Za-zА-Яа-яЁё\- ]*$");
        }

        public static bool IsPhoneChars(string text)
        {
            return text.All(ch => (ch >= '0' && ch <= '9') || ch == '+');
        }

        // Только английская раскладка: печатные ASCII-символы без пробела
        public static bool IsPasswordChars(string text)
        {
            return text.All(ch => ch >= 33 && ch <= 126);
        }

        // ---- Итоговые проверки: null, если всё верно, иначе текст ошибки ----
        public static string ValidateLogin(string login)
        {
            if (string.IsNullOrWhiteSpace(login))
                return "Введите логин.";
            if (!Regex.IsMatch(login, @"^[A-Za-z0-9_]{3,30}$"))
                return "Логин: от 3 до 30 символов, только латинские буквы, цифры и «_».";
            return null;
        }

        public static string ValidateFullName(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName))
                return "Введите ФИО.";
            if (fullName.Length < 5 || fullName.Length > 100 ||
                !Regex.IsMatch(fullName, @"^[A-Za-zА-Яа-яЁё\-]+( [A-Za-zА-Яа-яЁё\-]+)+$"))
                return "ФИО: минимум два слова, только буквы, пробел и дефис (5–100 символов).";
            return null;
        }

        // Телефон необязателен
        public static string ValidatePhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return null;
            if (!Regex.IsMatch(phone, @"^\+7\d{10}$"))
                return "Телефон должен быть в формате +7XXXXXXXXXX.";
            return null;
        }

        public static string ValidatePassword(string password)
        {
            if (string.IsNullOrEmpty(password) || password.Length < 6)
                return "Пароль слишком короткий: минимум 6 символов.";
            if (!IsPasswordChars(password))
                return "Используйте только английскую раскладку.";
            if (!password.Any(ch => ch >= '0' && ch <= '9'))
                return "Добавьте хотя бы одну цифру.";
            return null;
        }
    }
}