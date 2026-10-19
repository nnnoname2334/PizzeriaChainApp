using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using PizzeriaChainApp.Data;

namespace PizzeriaChainApp.Helpers
{
    public static class PizzeriaService
    {
        public static List<pizzeria> GetAll()
        {
            using (var db = AppData.CreateContext())
            {
                return db.pizzeria.AsNoTracking()
                    .OrderBy(p => p.city).ThenBy(p => p.address)
                    .ToList();
            }
        }

        // Сколько заказов будет удалено каскадом вместе с пиццерией
        public static int CountOrders(int pizzeriaId)
        {
            using (var db = AppData.CreateContext())
            {
                return db.customer_order.Count(o => o.pizzeria_id == pizzeriaId);
            }
        }

        // id == null: новая пиццерия, иначе правка существующей
        public static void Save(int? id, string name, string address, string city, string phone)
        {
            name = name.Trim();
            address = address.Trim();
            city = city.Trim();
            phone = phone.Trim();
            int currentId = id ?? 0;

            using (var db = AppData.CreateContext())
            {
                if (db.pizzeria.Any(p => p.id != currentId && p.name == name
                                      && p.address == address && p.city == city))
                    throw new InvalidOperationException("Такая пиццерия уже существует.");

                pizzeria item;
                if (id.HasValue)
                {
                    item = db.pizzeria.Find(id.Value);
                    if (item == null)
                        throw new InvalidOperationException("Пиццерия не найдена: возможно, она уже удалена.");
                }
                else
                {
                    item = new pizzeria();
                    db.pizzeria.Add(item);
                }

                item.name = name;
                item.address = address;
                item.city = city;
                item.phone = phone;
                db.SaveChanges();
            }
        }

        // Заказы пиццерии удаляются каскадно (FK ... ON DELETE CASCADE)
        public static void Delete(int id)
        {
            using (var db = AppData.CreateContext())
            {
                var item = db.pizzeria.Find(id);
                if (item != null)
                {
                    db.pizzeria.Remove(item);
                    db.SaveChanges();
                }
            }
        }
    }
}