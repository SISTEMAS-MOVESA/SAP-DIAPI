namespace IntegracionesSAP.Models.Purchasing
{
    public class PQT1
    {
        public int? LineNum { get; set; }

        public string? ItemCode { get; set; }
        public string? ItemDescription { get; set; }

        public string? AccountCode { get; set; }

        public double Quantity { get; set; } = 0;

        public double UnitPrice { get; set; } = 0;

        public string? TaxCode { get; set; } = "EXE";

        public List<UserField> UserFields { get; set; } = new List<UserField>();
    }
}
