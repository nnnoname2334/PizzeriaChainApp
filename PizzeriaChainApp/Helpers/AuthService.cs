using System.Data.Entity;
using System.Linq;
using PizzeriaChainApp.Data;

namespace PizzeriaChainApp.Helpers
{
    public static class AuthService
    {
        // Возвращает пользователя, если логин и пароль верны, иначе null
        public static customer Authenticate(string login, string password)
        {
            using (var db = AppData.CreateContext())
            {
                var user = db.customer.AsNoTracking().FirstOrDefault(c => c.login == login);
                if (user == null)
                    return null;
                return PasswordHelper.Verify(password, user.salt, user.password_hash) ? user : null;
            }
        }

        public static bool LoginExists(string login)
        {
            using (var db = AppData.CreateContext())
            {
                return db.customer.AsNoTracking().Any(c => c.login == login);
            }
        }

        public static customer Register(string login, string password, string fullName,
                                        string phone, string role)
        {
            string salt = PasswordHelper.GenerateSalt();
            var user = new customer
            {
                login = login,
                salt = salt,
                password_hash = PasswordHelper.HashPassword(password, salt),
                role = role,
                full_name = fullName,
                phone = string.IsNullOrWhiteSpace(phone) ? null : phone,
                bonus_points = 0
            };

            using (var db = AppData.CreateContext())
            {
                db.customer.Add(user);
                db.SaveChanges();
            }
            return user;
        }

        // true, если логин и текущий пароль верны и пароль изменён
        public static bool ChangePassword(string login, string oldPassword, string newPassword)
        {
            using (var db = AppData.CreateContext())
            {
                var user = db.customer.FirstOrDefault(c => c.login == login);
                if (user == null ||
                    !PasswordHelper.Verify(oldPassword, user.salt, user.password_hash))
                    return false;

                user.salt = PasswordHelper.GenerateSalt();
                user.password_hash = PasswordHelper.HashPassword(newPassword, user.salt);
                db.SaveChanges();
                return true;
            }
        }
    }
}