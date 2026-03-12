namespace IntegracionesSAP.Models.Sales
{
    public class RDR1
    {
        public int LineNum { get; set; }
        public string ItemCode { get; set; }
        public double Quantity { get; set; }
        public double Price { get; set; }
        public int? SysSerial { get; set; }
        public string? SerialNumber { get; set; }

    }
}
