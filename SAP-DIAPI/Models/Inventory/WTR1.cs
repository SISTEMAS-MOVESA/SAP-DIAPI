using SAPbobsCOM;

namespace IntegracionesSAP.Models.Inventory
{
    public class WTR1
    {
        public int? BaseEntry { get; set; }
        public int? BaseLine { get; set; }
        public int? BaseType { get; set; } = (int)InvBaseDocTypeEnum.InventoryTransferRequest;

        public int LineNum { get; set; }
        public string? ItemCode { get; set; }
        public double Quantity { get; set; }

        public int? SysSerial { get; set; }
        public string? SerialNumber { get; set; }
    }
}
