using Microsoft.AspNetCore.Mvc;
using StatisticEI.Services;
using StatisticEI.ViewModels;
using System.Globalization;

namespace StatisticEI.Controllers
{
    public class InvStatisticController : Controller
    {
        private readonly InvStatisticService _invStatisticService;
        public InvStatisticController(InvStatisticService invoiceService)
        {
            _invStatisticService = invoiceService;
        }
        public IActionResult Index(string date)
        {
            DateTime statisticDate = string.IsNullOrEmpty(date) ? DateTime.Now.AddDays(-1) : DateTime.ParseExact(date, "dd/MM/yyyy", CultureInfo.InvariantCulture);
            var vm = new InvStatisticViewModel()
            {
                StatisticDate = statisticDate,
                StatisticResult = _invStatisticService.GetStatistic(statisticDate)
            };
            return View(vm);
        }

        public IActionResult Statistic(string date)
        {
            DateTime statisticDate = DateTime.Parse(date);
            _invStatisticService.Statistic(statisticDate);
            return RedirectToAction(nameof(Index), statisticDate);
        }

        public IActionResult ExportErrorInvoice(string date)
        {
            DateTime statisticDate = DateTime.ParseExact(date, "dd/MM/yyyy", CultureInfo.InvariantCulture);
            var stream = _invStatisticService.ExportErrorInvoice(statisticDate);
            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "invoice_statistic_" + statisticDate + ".xlsx");
        }
    }
}
