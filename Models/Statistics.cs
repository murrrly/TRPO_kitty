namespace RPM.Models
{
    public class Statistics
    {
        public decimal DailyRevenue { get; set; }
        public int DailyOrders { get; set; }
        public int ActiveOrders { get; set; }
        public int NewCustomers { get; set; }
        public decimal AverageOrderValue { get; set; }
    }
}