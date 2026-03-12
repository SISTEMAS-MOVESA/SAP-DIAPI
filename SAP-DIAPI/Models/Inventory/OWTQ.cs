namespace IntegracionesSAP.Models.Inventory
{
    public class OWTQ
    {
        public string CardCode { get; set; }
        public string FromWarehouse { get; set; }
        public string ToWarehouse { get; set; }

        public int Series { get; set; }
        public int? TargetSeries { get; set; }

        public int PriceList { get; set; } = -2;
        public string? Comments { get; set; }

        public List<UserField> UserFields { get; set; } = new List<UserField>();

        public List<WTQ1> Lines { get; set; } = new List<WTQ1>();
    }
}
