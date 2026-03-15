namespace IntegracionesSAP.Models.Sales
{
    public class ODLN
    {
        public int? DocEntry { get; set; }
        public int? DocNum { get; set; }
        public string? CardCode { get; set; }
        public string? CardName { get; set; }
        public int Series { get; set; } = 0;
        public int TargetSeries { get; set; } = 0;
        public string? Comments { get; set; }

        public int SalesPersonCode { get; set; } = -1;

        public List<UserField> UserFields { get; set; } = new();

        public List<DLN1> Lines { get; set; } = new();
    }
}
