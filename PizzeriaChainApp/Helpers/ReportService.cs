using System.Collections.Generic;
using System.Linq;
using PizzeriaChainApp.Data;

namespace PizzeriaChainApp.Helpers
{
    public class CustomerReport
    {
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public List<OrderRow> Orders { get; set; }
    }

    public static class ReportService
    {
        public const string CancelledStatus = "Отменён";

        // Только клиенты, у которых есть заказы
        public static List<CustomerReport> Build()
        {
            return OrderService.GetOrders()
                .GroupBy(o => new { o.CustomerId, o.CustomerName })
                .OrderBy(g => g.Key.CustomerName)
                .Select(g => new CustomerReport
                {
                    CustomerId = g.Key.CustomerId,
                    CustomerName = g.Key.CustomerName,
                    Orders = g.OrderBy(o => o.OrderDate).ToList()
                })
                .ToList();
        }

        // Сумма заказов без отменённых
        public static decimal ActiveSum(IEnumerable<OrderRow> orders)
        {
            return orders.Where(o => o.Status != CancelledStatus).Sum(o => o.TotalAmount);
        }
    }
}