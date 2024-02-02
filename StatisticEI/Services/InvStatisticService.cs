using Dapper;
using Microsoft.Data.SqlClient;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using Polly;
using Polly.Contrib.WaitAndRetry;
using StatisticEI.Models;
using System.Diagnostics;
using System.Text.Json;

namespace StatisticEI.Services
{
    public class InvStatisticService
    {
        private readonly ILogger<InvStatisticService> _logger;
        private readonly IHostEnvironment _hostEnvironment;

        public InvStatisticService(ILogger<InvStatisticService> logger,
            IHostEnvironment hostEnvironment)
        {
            _logger = logger;
            _hostEnvironment = hostEnvironment;
        }

        public void Statistic(DateTime date)
        {
            var allDbs = ConfigUtils.GeAllConnectionString().Select(m => new DbInfo(m));
            InvStatisticResult result = new() { DbStatistics = new List<DbStatistic>() };
            var lockObj = new object();
            //Parallel.ForEach(allDbs, (db) =>
            foreach (var db in allDbs)
            {
                DbStatistic dbStatistic = new()
                {
                    DbId = db.Id,
                };
                try
                {
                    var signedInvoices = GetSignedInv(db.ConnectionString, date);
                    Debug.WriteLine(db.ConnectionString);
                    var signedInvCount = signedInvoices.Count;

                    var successInv = GetTCTSuccessInv(signedInvoices);
                    var successInvCount = successInv.Count;

                    var processInv = GetTCTProcessInv(signedInvoices);
                    var processInvCount = processInv.Count;

                    var tctErrorInv = GetTCTErrorInv(signedInvoices);
                    var tctErrorInvCount = tctErrorInv.Count;

                    var errorInv = GetErrorInv(signedInvoices);
                    var errorInvCount = errorInv.Count;

                    dbStatistic.SignedInv = signedInvoices;
                    dbStatistic.SignedInvCount = signedInvCount;
                    dbStatistic.TCTSuccessInvCount = successInvCount;
                    dbStatistic.TCTProcessInvCount = processInvCount;
                    dbStatistic.TCTErrorInvCount = tctErrorInvCount;
                    dbStatistic.ErrorInvCount = errorInvCount;
                }
                catch (Exception ex)
                {
                    dbStatistic.HasError = true;
                    dbStatistic.Note = ex.Message;
                }
                finally
                {
                    lock (lockObj)
                    {
                        result.DbStatistics.Add(dbStatistic);
                    }
                }
            }
            //});
            SaveResult(result, date);
        }

        private List<InvoiceBase> GetTCTSuccessInv(List<InvoiceBase> signedInvoices)
        {
            return signedInvoices.Where(m => m.No > 0 && m.Status > 0 && m.TCTCheckStatus == 1).ToList();
        }

        private List<InvoiceBase> GetTCTProcessInv(List<InvoiceBase> signedInvoices)
        {
            return signedInvoices.Where(m => m.No > 0 && m.Status > 0 && m.TCTCheckStatus == -1).ToList();
        }

        private List<InvoiceBase> GetTCTErrorInv(List<InvoiceBase> signedInvoices)
        {
            return signedInvoices.Where(m => m.No > 0 && m.Status > 0 && m.TCTCheckStatus == -2).ToList();
        }

        private List<InvoiceBase> GetErrorInv(List<InvoiceBase> signedInvoices)
        {
            int?[] tctCheckStatus = new int?[] { -2, -1, 1 };
            return signedInvoices.Where(m => !(m.No > 0 && m.Status > 0 && tctCheckStatus.Contains(m.TCTCheckStatus))).ToList();
        }

        private List<InvoiceBase> GetSignedInv(string connectionString, DateTime date)
        {
            string sql = $@"select i.no, i.ComID, i.pattern, i.status, i.Serial, i.TCTCheckStatus, i.PublishDate from VATInvoice i
                            where (i.status > 0 or i.no > 0) and i.ArisingDate = '{date:yyyy-MM-dd}'";
            List<InvoiceBase> invoices = new List<InvoiceBase>();

            var delay = Backoff.ConstantBackoff(TimeSpan.FromMilliseconds(1000), retryCount: 5);
            var policy = Policy.Handle<SqlException>(ex =>
            {
                return ex.Number == -2;
            }).WaitAndRetry(delay);

            invoices = policy.Execute(() =>
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    return connection.Query<InvoiceBase>(sql, commandTimeout: Constants.CommandTimeout).ToList();
                }
            });

            return invoices;
        }

        private void SaveResult(InvStatisticResult result, DateTime date)
        {
            string folder = Path.Combine(_hostEnvironment.ContentRootPath,
                  "StatisticalResults", "Inv");
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
            File.WriteAllText(Path.Combine(folder, date.ToString("yyyyMMdd") + ".json"), JsonSerializer.Serialize(result));
        }

        public Stream ExportErrorInvoice(DateTime date)
        {
            var statistic = GetStatistic(date);
            Stream stream = new MemoryStream();
            string templateExcelPath = Path.Combine(_hostEnvironment.ContentRootPath,
                    "Templates", "invoice_statistic.xlsx");

            using (ExcelPackage p = new(templateExcelPath))
            {
                ExcelWorksheet ws = p.Workbook.Worksheets["Sheet1"];
                int row = 2;

                foreach (var item in statistic?.DbStatistics ?? Enumerable.Empty<DbStatistic>())
                {
                    if (item.SignedInv == null)
                    {
                        continue;
                    }

                    var errorInvoices = GetErrorInv(item.SignedInv!).OrderBy(m => m.ComID).ThenBy(m => m.Pattern).ThenBy(m => m.No);

                    foreach (var errorInvoice in errorInvoices)
                    {
                        ws.Cells["A" + row].Value = item.DbId;
                        ws.Cells["B" + row].Value = errorInvoice.ComID;
                        ws.Cells["C" + row].Value = errorInvoice.Pattern;
                        ws.Cells["D" + row].Value = errorInvoice.Serial;
                        ws.Cells["E" + row].Value = errorInvoice.No;
                        ws.Cells["F" + row].Value = errorInvoice.Status;
                        ws.Cells["G" + row].Value = errorInvoice.PublishDate;
                        ws.Cells["H" + row].Value = errorInvoice.TCTCheckStatus;
                        row++;
                    }
                }

                p.SaveAs(stream);
                stream.Position = 0;
            }

            return stream;
        }

        public InvStatisticResult? GetStatistic(DateTime date)
        {
            string path = Path.Combine(_hostEnvironment.ContentRootPath, "StatisticalResults", "Inv");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            DirectoryInfo d = new(path);
            string fileName = date.ToString("yyyyMMdd") + ".json";
            var file = d.GetFiles("*.json").FirstOrDefault(m => m.Name == fileName);
            if (file is null)
                Statistic(date);
            var result = JsonSerializer.Deserialize<InvStatisticResult>(File.ReadAllText(path + "\\" + fileName));
            return result;
        }
    }
}
