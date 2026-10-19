namespace PizzeriaChainApp.Data
{
    public static class AppData
    {
        private static PizzeriaEntities _context;
        public static PizzeriaEntities GetContext()
        {
            if (_context == null)
                _context = new PizzeriaEntities();
            return _context;
        }

        // Текущий авторизованный пользователь
        public static customer CurrentUser { get; set; }
    }
}