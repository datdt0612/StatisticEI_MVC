using StatisticEI.Enums;

namespace StatisticEI.Models
{
    public class InvoiceBase
    {
        public int Id { get; set; }
        public DateTime ArisingDate { get; set; }
        public int ComID { get; set; }
        public DateTime ModifiedDate { get; set; }
        public decimal No { get; set; }
        public string Pattern { get; set; } = null!;
        public DateTime PublishDate { get; set; }
        public string Serial { get; set; } = null!;
        public InvoiceStatus Status { get; set; }
        public int? TCTCheckStatus { get; set; }
    }
}
