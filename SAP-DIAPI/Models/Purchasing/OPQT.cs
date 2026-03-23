namespace IntegracionesSAP.Models.Purchasing
{
    public class OPQT
    {
        public string? CardCode { get; set; }

        public int? Series { get; set; }
        public int? TargetSeries { get; set; }

        public DateTime DocDate { get; set; } = DateTime.Now;

        public DateTime DocDueDate { get; set; } = DateTime.Now;

        public string? Comments { get; set; }

        public string? JournalMemo { get; set; }

        public List<UserField> UserFields { get; set; } = new List<UserField>();

        public List<PQT1> Lines { get; set; } = new List<PQT1>();
    }
}
