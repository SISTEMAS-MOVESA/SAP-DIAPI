namespace IntegracionesSAP.Models.Purchasing
{
    public class POR1
    {
        public int? LineNum { get; set; }

        public string ItemDescription { get; set; }

        public string AccountCode { get; set; }

        public double Quantity { get; set; } = 1;

        public double UnitPrice { get; set; }

        public string TaxCode { get; set; }

        public List<UserField> UserFields { get; set; } = new List<UserField>();
    }
}
