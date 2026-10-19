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
    }
}