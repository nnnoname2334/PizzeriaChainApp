namespace PizzeriaChainApp.Data
{
    public static class AppData
    {
        private static PizzeriaEntities _context;

        // Общий контекст для чтения данных на страницах
        public static PizzeriaEntities GetContext()
        {
            if (_context == null)
                _context = new PizzeriaEntities();
            return _context;
        }

        // Новый контекст для коротких операций (вход, регистрация, смена пароля)
        public static PizzeriaEntities CreateContext()
        {
            return new PizzeriaEntities();
        }

        // Текущий авторизованный пользователь
        public static customer CurrentUser { get; set; }
    }
}