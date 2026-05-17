
namespace IntegracionesSAP.Models.Sales
{
    public class ORDR
    {
        public int? DocEntry { get; set; }
        public int? DocNum { get; set; }
        public DateTime DocDate { get; set; } = DateTime.Now;
        public DateTime DocDueDate { get; set; } = DateTime.Now;
        public DateTime TaxDate { get; set; } = DateTime.Now;
        public string? NumAtCard { get; set; }
        public int? PaymentGroupCode { get; set; }
        public double? DocTotal { get; set; }
        public string? CardCode { get; set; }
        public string? CardName { get; set; }
        public string? Comments { get; set; }
        public int Series { get; set; } = 0;
        public int TargetSeries { get; set; } = 0;
        public int TargetSeries2 { get; set; } = 0;
        public int SalesPersonCode { get; set; } = -1;
        public double? DiscPrcnt { get; set; }

        public List<UserField> UserFields { get; set; } = new();
        public List<RDR1> Lines { get; set; } = new();
    }
}
