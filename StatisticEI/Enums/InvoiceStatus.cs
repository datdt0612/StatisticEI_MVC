using System.ComponentModel;

namespace StatisticEI.Enums
{
    public enum InvoiceStatus
    {
        [Description("Hóa đơn chờ ký số")] Null = -1,
        [Description("Hóa đơn mới tạo lập")] NewInv = 0,
        [Description("Hóa đơn có chữ ký số")] SignedInv = 1,

        [Description("Hóa đơn đã khai báo thuế")]
        InUseInv = 2,
        [Description("Hóa đơn bị thay thế")] ReplacedInv = 3,
        [Description("Hóa đơn bị điều chỉnh")] AdjustedInv = 4,
        [Description("Hóa đơn bị hủy")] CanceledInv = 5,
        [Description("Hóa đơn đã duyệt")] ApprovedInv = 6,
    }

    public enum TCTCheckStatusInv
    {
        [Description("Đang kiểm tra")]
        Process = -1,
        [Description("Hợp lệ")]
        Success = 1,
        [Description("Không hợp lệ")]
        Error = -2
    }
}
