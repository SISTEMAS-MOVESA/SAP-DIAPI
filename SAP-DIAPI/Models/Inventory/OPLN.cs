namespace IntegracionesSAP.Models.Inventory
{
    public class OPLN
    {
        public int? ListNum { get; set; }
        public string? ListName { get; set; }

        /// <summary>Precio directo — usado en OITM.PriceLists para asignar precio por lista al crear un artículo.</summary>
        public double? Price { get; set; }

        public List<OITM> Items { get; set; } = new();
    }
}
