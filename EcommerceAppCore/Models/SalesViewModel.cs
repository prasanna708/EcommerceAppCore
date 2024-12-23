namespace EcommerceAppCore.Models
{
    public class SalesViewModel
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public List<PurchaseAuditLog> FilteredSales { get; set; }
    }
}
