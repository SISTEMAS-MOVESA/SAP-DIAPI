namespace IntegracionesSAP.Models.Inventory
{
    public class WTQ1
    {
        public int LineNum { get; set; }
        public string ItemCode { get; set; }
        public double Quantity { get; set; }

        public int? SysSerial { get; set; }
        public string? SerialNumber { get; set; }
    }
}
