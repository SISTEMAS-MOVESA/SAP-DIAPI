namespace IntegracionesSAP.Models.Purchasing
{
    /// <summary>
    /// Línea de Factura de Proveedores (OPCH).
    /// BaseType: 20 = OPDN (Entrada Mercancía), 22 = OPOR (Pedido de Compra).
    /// </summary>
    public class PCH1
    {
        public int? LineNum { get; set; }

        // Artículo (DocType = dDocument_Items)
        public string? ItemCode { get; set; }

        // Descripción libre (DocType = dDocument_Service)
        public string? ItemDescription { get; set; }

        // Cuenta contable (requerida para DocType = dDocument_Service)
        public string? AccountCode { get; set; }

        public double Quantity { get; set; } = 0;
        public double UnitPrice { get; set; } = 0;
        public string? TaxCode { get; set; } = "EXE";

        // Documento base — BaseType: 20 (OPDN) o 22 (OPOR)
        public int? BaseEntry { get; set; }
        public int? BaseLine { get; set; }
        public int? BaseType { get; set; }

        public List<UserField>? UserFields { get; set; }
    }
}
