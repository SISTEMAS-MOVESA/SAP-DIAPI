namespace IntegracionesSAP.Models.Purchasing
{
    public class OPDN
    {
        public string CardCode { get; set; } = string.Empty;
        public DateTime? DocDate { get; set; }
        public DateTime? DocDueDate { get; set; }

        public string DocCurrency { get; set; }
        public double DocTotal { get; set; }
        public string NumAtCard { get; set; }

        public int? Series { get; set; }
        public string? Comments { get; set; }

        public List<UserField>? UserFields { get; set; }
        public List<PDN1> Lines { get; set; } = new();
    }
}
