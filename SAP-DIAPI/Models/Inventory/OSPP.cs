using IntegracionesSAP.Models.Business_Partners;

namespace IntegracionesSAP.Models.Inventory
{
    public class OSPP
    {
        public string? CardCode { get; set; }
        public List<OCRD> CardCodes { get; set; } = new();
        public List<OITM> Items { get; set; } = new();

        public string? Currency { get; set; }
    }
}
