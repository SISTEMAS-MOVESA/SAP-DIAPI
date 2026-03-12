
namespace IntegracionesSAP.Models.Sales
{
    public class ORDR
    {
        public string CardCode { get; set; }
        public string Comments { get; set; }
        public int Series { get; set; }
        public int SalesPersonCode { get; set; }

        public List<UserField> UserFields { get; set; } = new();
        public List<RDR1> Lines { get; set; } = new();
    }
}
