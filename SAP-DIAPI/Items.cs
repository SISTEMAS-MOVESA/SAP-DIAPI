using System.Numerics;

namespace IntegracionesSAP
{
    public class ItemPriceUpdate
    {
        public string? ItemCode { get; set; }
        public int PriceList { get; set; }
        public decimal Price { get; set; }
        public string? Currency { get; set; }

        // Resultado que devolverá el servicio SAP
        public string? ResultMessage { get; set; }
        public int SapResultCode { get; set; }
    }

    public class ItemPriceUpdateBatch
    {
        public List<ItemPriceUpdate> Items { get; set; } = new();
    }

    public class ItemCreate
    {
        public string? ItemCode { get; set; }
        public string? ItemName { get; set; }
        public int ItemsGroupCode { get; set; }
        public string? Manufacturer { get; set; }

        // === Flags de SAP (tYES / tNO) → representados como booleanos en API ===
        public bool VatLiable { get; set; }
        public bool PurchaseItem { get; set; }
        public bool SalesItem { get; set; }
        public bool InventoryItem { get; set; }
        public bool ManageSerialNumbers { get; set; }

        // === Campos opcionales ===
        public string? ForeignName { get; set; }
        public string? BarCode { get; set; }
        public string? MainSupplier { get; set; }
        public string? SupplierCatalogNo { get; set; }

        // === Unidades de venta ===
        public string? SalesUnit { get; set; }
        public decimal? SalesItemsPerUnit { get; set; }
        public decimal? SalesPackagingUnit { get; set; }
        public decimal? SalesQtyPerPackUnit { get; set; }

        // === Unidades de compra ===
        public string? PurchaseUnit { get; set; }
        public decimal? PurchaseItemsPerUnit { get; set; }
        public decimal? PurchasePackagingUnit { get; set; }
        public decimal? PurchaseQtyPerPackUnit { get; set; }

        // === Dimensiones ===
        public decimal? PurchaseUnitLength { get; set; }
        public decimal? PurchaseUnitWidth { get; set; }
        public decimal? PurchaseUnitHeight { get; set; }
        public int? UnidadMedidaDimensiones { get; set; }

        // === Peso ===
        public decimal? Peso { get; set; }
        public int? UnidadMedidaPeso { get; set; }

        // Mensaje de resultado (opcional para retorno)
        public string? ResultMessage { get; set; }
    }
}
