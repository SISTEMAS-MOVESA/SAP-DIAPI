using SAPbobsCOM;

namespace IntegracionesSAP.Models.Business_Partners
{
    public class CRD1
    {
        public string AddressName { get; set; }
        public string? Street { get; set; }
        public string? Block { get; set; }
        public string? AddressName3 { get; set; }

        public string? City { get; set; }
        public string? County { get; set; }
        public string? State { get; set; }
        public string? Country { get; set; } = "HN";

        public BoAddressType AddressType { get; set; } = BoAddressType.bo_ShipTo;

        public string? TaxCode { get; set; }
    }
}
