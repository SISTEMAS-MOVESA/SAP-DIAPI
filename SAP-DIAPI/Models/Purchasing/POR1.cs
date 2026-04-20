namespace IntegracionesSAP.Models.Purchasing
{
    public class POR1
    {
        public int? LineNum { get; set; }

        public string? ItemCode { get; set; }
        public string? ItemDescription { get; set; }

        public string? AccountCode { get; set; }

        public double Quantity { get; set; } = 0;

        public double UnitPrice { get; set; } = 0;

        public string? TaxCode { get; set; } = "EXE";

        /// <summary>Código de almacén (solo para DocType = dDocument_Items).</summary>
        public string? WhsCode { get; set; }

        // ── Referencia a documento base (opcional) ──────────────────────────
        // BaseType: 540000006 = OPQT, 22 = OPOR, 20 = OPDN, 1470000113 = OPRQ
        // Cuando BaseEntry > 0 SAP copia ItemCode y datos del documento origen.
        // Quantity = 0 → SAP usa la cantidad abierta restante automáticamente.
        public int? BaseEntry { get; set; }
        public int? BaseLine  { get; set; }
        public int? BaseType  { get; set; }

        public List<UserField> UserFields { get; set; } = new List<UserField>();
    }
}
