using StatisticEI.Models;

namespace StatisticEI.ViewModels
{
    public class InvStatisticViewModel
    {
        public DateTime StatisticDate { get; set; }
        public List<DbInfo> Dbs { get; set; } = new List<DbInfo>();
        public InvStatisticResult? StatisticResult { get; set; }
    }
}
