namespace IntegracionesSAP.Models.Business_Partners
{
    public class OCRD
    {
        public string? CardCode { get; set; }
        public string? CardName { get; set; }
        public string? CardForeignName { get; set; }

        public string? Currency { get; set; } = "LPS";
        public int GroupCode { get; set; } = 0;

        public string? FederalTaxID { get; set; }
        public string? AdditionalID { get; set; }
        public string? UnifiedFederalTaxID { get; set; }

        public string? Phone1 { get; set; }
        public string? Phone2 { get; set; }
        public string? Cellular { get; set; }

        public string? EmailAddress { get; set; }
        public string? Notes { get; set; }

        public int PayTermsGrpCode { get; set; } = 0;
        public int PriceListNum { get; set; } = -1;

        public int SalesPersonCode { get; set; } = -1;

        public bool Frozen { get; set; } = false;
        public DateTime? FrozenFrom { get; set; }
        public bool Valid { get; set; } = true;

        public string? FatherCard { get; set; }

        public List<CRD1> Addresses { get; set; } = new();
        public List<UserField> UserFields { get; set; } = new();
        public List<SapProperty> Properties { get; set; } = new();
    }
}
