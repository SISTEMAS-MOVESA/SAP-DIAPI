namespace IntegracionesSAP.Models.Inventory
{
    public class OSPP
    {
        public string? CardCode { get; set; }
        public List<OITM> Items { get; set; } = new();
    }
}
