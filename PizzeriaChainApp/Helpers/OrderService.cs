using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using PizzeriaChainApp.Data;

namespace PizzeriaChainApp.Helpers
{
    public static class OrderService
    {
        // Заказы одного клиента (customerId) или все заказы (null)
        public static List<OrderRow> GetOrders(int? customerId = null)
        {
            using (var db = AppData.CreateContext())
            {
                IQueryable<customer_order> query = db.customer_order.AsNoTracking();

                if (customerId.HasValue)
                {
                    int id = customerId.Value;
                    query = query.Where(o => o.customer_id == id);
                }

                return query
                    .OrderByDescending(o => o.order_date)
                    .Select(o => new OrderRow
                    {
                        Id = o.id,
                        CustomerId = o.customer_id,
                        PizzeriaId = o.pizzeria_id,
                        CustomerName = o.customer.full_name,
                        PizzeriaName = o.pizzeria.name,
                        Address = o.pizzeria.address,
                        City = o.pizzeria.city,
                        OrderDate = o.order_date,
                        OrderType = o.order_type,
                        Status = o.status,
                        TotalAmount = o.total_amount
                    })
                    .ToList();
            }
        }

        // Поиск, фильтры и сортировка. orderType/status = null означает «все».
        // sortIndex: 0 - новые сначала, 1 - старые сначала,
        //            2 - сумма по возрастанию, 3 - сумма по убыванию
        public static List<OrderRow> Filter(IEnumerable<OrderRow> source, string search,
            string orderType, string status, int sortIndex, bool searchByCustomer = false)
        {
            IEnumerable<OrderRow> query = source;

            if (!string.IsNullOrWhiteSpace(search))
            {
                string s = search.Trim();
                query = query.Where(o => Contains(o.PizzeriaName, s)
                                      || Contains(o.Address, s)
                                      || Contains(o.City, s)
                                      || (searchByCustomer && Contains(o.CustomerName, s)));
            }

            if (!string.IsNullOrEmpty(orderType))
                query = query.Where(o => o.OrderType == orderType);

            if (!string.IsNullOrEmpty(status))
                query = query.Where(o => o.Status == status);

            switch (sortIndex)
            {
                case 1: query = query.OrderBy(o => o.OrderDate); break;
                case 2: query = query.OrderBy(o => o.TotalAmount); break;
                case 3: query = query.OrderByDescending(o => o.TotalAmount); break;
                default: query = query.OrderByDescending(o => o.OrderDate); break;
            }

            return query.ToList();
        }

        private static bool Contains(string value, string text)
        {
            return value != null &&
                   value.IndexOf(text, StringComparison.CurrentCultureIgnoreCase) >= 0;
        }
        // Списки для выпадающих полей формы заказа
        public static List<LookupItem> GetCustomerLookup()
        {
            using (var db = AppData.CreateContext())
            {
                return db.customer.AsNoTracking()
                    .OrderBy(c => c.full_name)
                    .Select(c => new LookupItem
                    {
                        Id = c.id,
                        Name = c.full_name + " (" + c.login + ")"
                    })
                    .ToList();
            }
        }

        public static List<LookupItem> GetPizzeriaLookup()
        {
            using (var db = AppData.CreateContext())
            {
                return db.pizzeria.AsNoTracking()
                    .OrderBy(p => p.city).ThenBy(p => p.address)
                    .Select(p => new LookupItem
                    {
                        Id = p.id,
                        Name = p.name + ", " + p.city + ", " + p.address
                    })
                    .ToList();
            }
        }

        // id == null: новый заказ, иначе правка существующего
        public static void SaveOrder(int? id, int customerId, int pizzeriaId, DateTime date,
                                     string orderType, string status, decimal total)
        {
            using (var db = AppData.CreateContext())
            {
                customer_order order;
                if (id.HasValue)
                {
                    order = db.customer_order.Find(id.Value);
                    if (order == null)
                        throw new InvalidOperationException("Заказ не найден: возможно, он уже удалён.");
                }
                else
                {
                    order = new customer_order();
                    db.customer_order.Add(order);
                }

                order.customer_id = customerId;
                order.pizzeria_id = pizzeriaId;
                order.order_date = date;
                order.order_type = orderType;
                order.status = status;
                order.total_amount = total;
                db.SaveChanges();
            }
        }

        public static void DeleteOrder(int id)
        {
            using (var db = AppData.CreateContext())
            {
                var order = db.customer_order.Find(id);
                if (order != null)
                {
                    db.customer_order.Remove(order);
                    db.SaveChanges();
                }
            }
        }
    }
}