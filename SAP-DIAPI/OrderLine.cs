namespace IntegracionesSAP
{
    public class OrderLine
    {
        public int LineNum { get; set; }
        public string ItemCode { get; set; }
        public double Quantity { get; set; }
        public double Price { get; set; }
        public string? WhsCode { get; set; }
        public string? TaxCode { get; set; }
        public double DiscountPercent { get; set; }

        // === UserFields de motos ===
        public string? U_MSERIE { get; set; }
        public string? U_MColor { get; set; }
        public string? U_MAno { get; set; }
        public string? U_MModelo { get; set; }
        public string? U_DatosAduana { get; set; }
        public string? U_MMarca { get; set; }
        public string? U_MItem { get; set; }

        public bool Serialized { get; set; } = false;

        // === Campos adicionales según VB ===
        public string? Currency { get; set; }
        public int? BaseEntry { get; set; }
        public int? BaseLine { get; set; }
        public int? BaseType { get; set; }

        // === Seriales (nueva subclase) ===
        public List<ItemSerial> Serials { get; set; } = new List<ItemSerial>();

        //nuevos campos
        public string? SerialNum { get; set; }
        public string? U_SerieMotosAsig { get; set; }
        public string? U_Precio_Placa { get; set; }
        public string? U_N_Factura { get; set; }
    }

    public class ItemSerial
    {
        public string? ManufacturerSerialNumber { get; set; }
        public string? InternalSerialNumber { get; set; }
        public DateTime? ManufactureDate { get; set; }
        public string? Location { get; set; }
        public string? Notes { get; set; }
        public string? BatchID { get; set; }
        public string? U_UBICACION_DCM { get; set; }
        public int? DocLineNum { get; set; }

    }
}
