using System.Linq;
using System.Text.RegularExpressions;
using System;
using System.Globalization;

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
        // ---- Заказы: ограничения числовых полей ----
        public const decimal MinAmount = 50m;
        public const decimal MaxAmount = 100000m;

        public static bool IsAmountChars(string text)
        {
            return text.All(ch => (ch >= '0' && ch <= '9') || ch == ',' || ch == '.');
        }

        public static bool IsDateTimeChars(string text)
        {
            return text.All(ch => (ch >= '0' && ch <= '9') || ch == '.' || ch == ':' || ch == ' ');
        }

        public static string ValidateAmount(string text, out decimal value)
        {
            value = 0;
            if (string.IsNullOrWhiteSpace(text))
                return "Введите сумму заказа.";

            string normalized = text.Trim().Replace('.', ',');
            if (!decimal.TryParse(normalized, NumberStyles.AllowDecimalPoint,
                                  new CultureInfo("ru-RU"), out value))
                return "Сумма указана в неверном формате.";
            if (value < MinAmount || value > MaxAmount)
                return "Сумма заказа должна быть от " + MinAmount + " до " + MaxAmount + " ₽.";
            if (decimal.Round(value, 2) != value)
                return "Сумма: не более двух знаков после запятой.";
            return null;
        }

        public static string ValidateOrderDate(string text, out DateTime value)
        {
            value = DateTime.MinValue;
            if (string.IsNullOrWhiteSpace(text) ||
                !DateTime.TryParseExact(text.Trim(), "dd.MM.yyyy HH:mm",
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out value))
                return "Дата и время: формат дд.мм.гггг чч:мм.";
            if (value < new DateTime(2020, 1, 1) || value > DateTime.Now)
                return "Дата заказа: от 01.01.2020 до текущего момента.";
            return null;
        }
        // ---- Пиццерии и клиенты ----
        public const int MinBonus = 0;
        public const int MaxBonus = 100000;

        public static bool IsDigitsOnly(string text)
        {
            return text.All(ch => ch >= '0' && ch <= '9');
        }

        public static bool IsTitleChars(string text)
        {
            return Regex.IsMatch(text, @"^[A-Za-zА-Яа-яЁё0-9\-\.«»"" ]*$");
        }

        public static bool IsAddressChars(string text)
        {
            return Regex.IsMatch(text, @"^[A-Za-zА-Яа-яЁё0-9\-\.,/ ]*$");
        }

        public static bool IsEmailChars(string text)
        {
            return text.All(ch => ch >= 33 && ch <= 126);
        }

        public static string ValidatePizzeriaName(string name)
        {
            string n = name == null ? "" : name.Trim();
            if (n.Length < 2 || n.Length > 100)
                return "Название пиццерии: от 2 до 100 символов.";
            return null;
        }

        public static string ValidateAddress(string address)
        {
            string a = address == null ? "" : address.Trim();
            if (a.Length < 5 || a.Length > 200)
                return "Адрес: от 5 до 200 символов.";
            return null;
        }

        public static string ValidateCity(string city)
        {
            string c = city == null ? "" : city.Trim();
            if (c.Length < 2 || c.Length > 100 ||
                !Regex.IsMatch(c, @"^[A-Za-zА-Яа-яЁё]+([\- ][A-Za-zА-Яа-яЁё]+)*$"))
                return "Город: от 2 до 100 символов, только буквы, пробел и дефис.";
            return null;
        }

        public static string ValidateRequiredPhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return "Введите телефон.";
            return ValidatePhone(phone);
        }

        // Email необязателен
        public static string ValidateEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return null;
            if (email.Trim().Length > 100 ||
                !Regex.IsMatch(email.Trim(), @"^[A-Za-z0-9._%+\-]+@[A-Za-z0-9.\-]+\.[A-Za-z]{2,}$"))
                return "Email указан в неверном формате.";
            return null;
        }

        public static string ValidateBonus(string text, out int value)
        {
            value = 0;
            if (string.IsNullOrWhiteSpace(text))
                return "Введите количество бонусов.";
            if (!int.TryParse(text.Trim(), out value))
                return "Бонусы: введите целое число.";
            if (value < MinBonus || value > MaxBonus)
                return "Бонусы: от " + MinBonus + " до " + MaxBonus + ".";
            return null;
        }
    }
}