using IntegracionesSAP.Models.Inventory;

namespace IntegracionesSAP.Models.Purchasing
{
    public class PDN1
    {
        public string ItemCode { get; set; } = string.Empty;
        public double Quantity { get; set; }
        public double UnitPrice { get; set; }
        public string WhsCode { get; set; } = string.Empty;
        public string TaxCode { get; set; } = string.Empty;

        public int? BaseEntry { get; set; }
        public int? BaseLine { get; set; }
        public int? BaseType { get; set; }

        public List<OSRN> Serials { get; set; } = new();
        public List<UserField>? UserFields { get; set; }
    }
}
