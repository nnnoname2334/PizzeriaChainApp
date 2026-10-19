using System;

namespace PizzeriaChainApp.Data
{
    public class OrderRow
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public int PizzeriaId { get; set; }
        public string CustomerName { get; set; }
        public string PizzeriaName { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public DateTime OrderDate { get; set; }
        public string OrderType { get; set; }
        public string Status { get; set; }
        public decimal TotalAmount { get; set; }

        public string FullAddress
        {
            get { return City + ", " + Address; }
        }
    }
}