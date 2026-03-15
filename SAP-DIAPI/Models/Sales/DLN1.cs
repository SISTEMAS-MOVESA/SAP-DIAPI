namespace IntegracionesSAP.Models.Sales
{
    public class DLN1
    {
        public int? LineNum { get; set; }

        public string? ItemCode { get; set; }

        public double Quantity { get; set; } = 0;

        public double? Price { get; set; }

        public int? BaseEntry { get; set; }

        public int? BaseLine { get; set; }

        public string? SerialNumber { get; set; }

        public int? SysSerial { get; set; }

        public List<UserField> UserFields { get; set; } = new();
    }
}
