using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using PizzeriaChainApp.Data;

namespace PizzeriaChainApp.Helpers
{
    public static class CustomerService
    {
        public static List<customer> GetAll()
        {
            using (var db = AppData.CreateContext())
            {
                return db.customer.AsNoTracking().OrderBy(c => c.full_name).ToList();
            }
        }

        public static int CountOrders(int customerId)
        {
            using (var db = AppData.CreateContext())
            {
                return db.customer_order.Count(o => o.customer_id == customerId);
            }
        }

        public static void Update(int id, string fullName, string phone, string email,
                                  int bonusPoints, string role)
        {
            using (var db = AppData.CreateContext())
            {
                var item = db.customer.Find(id);
                if (item == null)
                    throw new InvalidOperationException("Клиент не найден: возможно, он уже удалён.");

                // Нельзя оставить систему без администратора
                if (item.role == "Admin" && role != "Admin" &&
                    db.customer.Count(c => c.role == "Admin") <= 1)
                    throw new InvalidOperationException("Нельзя снять роль у последнего администратора.");

                item.full_name = fullName.Trim();
                item.phone = string.IsNullOrWhiteSpace(phone) ? null : phone.Trim();
                item.email = string.IsNullOrWhiteSpace(email) ? null : email.Trim();
                item.bonus_points = bonusPoints;
                item.role = role;
                db.SaveChanges();
            }
        }

        // Заказы клиента удаляются каскадно
        public static void Delete(int id)
        {
            if (AppData.CurrentUser != null && AppData.CurrentUser.id == id)
                throw new InvalidOperationException("Нельзя удалить собственную учётную запись.");

            using (var db = AppData.CreateContext())
            {
                var item = db.customer.Find(id);
                if (item == null)
                    return;

                if (item.role == "Admin" && db.customer.Count(c => c.role == "Admin") <= 1)
                    throw new InvalidOperationException("Нельзя удалить последнего администратора.");

                db.customer.Remove(item);
                db.SaveChanges();
            }
        }
    }
}