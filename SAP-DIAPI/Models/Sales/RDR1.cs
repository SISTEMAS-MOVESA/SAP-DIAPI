namespace IntegracionesSAP.Models.Sales
{
    public class RDR1
    {
        public int? LineNum { get; set; }
        public string? ItemCode { get; set; }
        public string? WhsCode { get; set; }
        public double Quantity { get; set; } = 0;
        public double? DiscountPercent { get; set; }
        public double? Price { get; set; }
        public int? SysSerial { get; set; }
        public string? SerialNumber { get; set; }
        public string TaxCode { get; set; } = "ISV";

        public List<UserField> UserFields { get; set; } = new();

    }
}
