namespace IntegracionesSAP.Models.Sales
{
    public class OINV
    {
        public string? CardCode { get; set; }

        public int? Series { get; set; }

        public string? Comments { get; set; }

        public int SalesPersonCode { get; set; } = -1;

        public List<UserField> UserFields { get; set; } = new();

        public List<INV1> Lines { get; set; } = new();
    }
}
