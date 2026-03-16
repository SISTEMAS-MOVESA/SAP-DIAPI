namespace IntegracionesSAP.Models.Inventory
{
    public class OPLN
    {
        public int? ListNum { get; set; }
        public string? ListName { get; set; }
        public List<OITM> Items { get; set; } = new();
    }
}
