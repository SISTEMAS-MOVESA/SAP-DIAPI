namespace IntegracionesSAP.Models.Inventory
{
    public class OSRN
    {
        public string InternalSerialNumber { get; set; } = string.Empty;

        public string ManufacturerSerialNumber { get; set; } = string.Empty;

        public DateTime? ManufactureDate { get; set; }

        public string? Location { get; set; }

        public string? Notes { get; set; }

        public List<UserField>? UserFields { get; set; }
    }
}
