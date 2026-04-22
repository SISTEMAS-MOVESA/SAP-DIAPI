namespace IntegracionesSAP.Models.Inventory
{
    public class OSRN
    {
        public string? InternalSerialNumber { get; set; }
        public string? ManufacturerSerialNumber { get; set; }
        public DateTime ManufactureDate { get; set; } = DateTime.Now;

        public string? Location { get; set; }
        public string? Notes { get; set; }
        public string? BatchID { get; set; }

        public List<UserField>? UserFields { get; set; }
    }
}
