namespace StatisticEI.Models
{
    public class DbStatistic
    {
        public string DbId { get; set; } = null!;
        public List<InvoiceBase>? SignedInv { get; set; }
        public int? SignedInvCount { get; set; }
        public int? TCTSuccessInvCount { get; set; }
        public int? TCTProcessInvCount { get; set; }
        public int? TCTErrorInvCount { get; set; }
        public int? ErrorInvCount { get; set; }
        public string? Note { get; set; }
        public bool HasError { get; set; }
    }
}
