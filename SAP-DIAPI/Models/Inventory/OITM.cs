using IntegracionesSAP.Models;

namespace IntegracionesSAP.Models.Inventory
{
    public class OITM
    {
        // ── Identificación ──────────────────────────────────────────────
        /// <summary>Código del artículo (ItemCode)</summary>
        public string? ItemCode { get; set; }

        /// <summary>Nombre del artículo (ItemName)</summary>
        public string? ItemName { get; set; }

        /// <summary>Nombre extranjero (FrgnName)</summary>
        public string? ForeignName { get; set; }

        // ── Clasificación ───────────────────────────────────────────────
        /// <summary>Código grupo de artículos (ItmsGrpCod)</summary>
        public int? ItemsGroupCode { get; set; }

        /// <summary>Código grupo aduanero (CstGrpCode)</summary>
        public int? CustomsGroupCode { get; set; }

        /// <summary>Código de barras (CodeBars)</summary>
        public string? BarCode { get; set; }

        // ── Flags de comportamiento (Y/N) ───────────────────────────────
        /// <summary>Sujeto a IVA (VATLiable)</summary>
        public string? VATLiable { get; set; }

        /// <summary>Artículo de compra (PrchseItem)</summary>
        public string? PurchaseItem { get; set; }

        /// <summary>Artículo de venta (SellItem)</summary>
        public string? SalesItem { get; set; }

        /// <summary>Artículo de inventario (InvntItem)</summary>
        public string? InventoryItem { get; set; }

        /// <summary>Gestionar stock por almacén (ByWh)</summary>
        public string? ManageStockByWarehouse { get; set; }

        // ── Proveedor ───────────────────────────────────────────────────
        /// <summary>Proveedor principal — CardCode (CardCode)</summary>
        public string? Mainsupplier { get; set; }

        /// <summary>Número de catálogo del proveedor (SuppCatNum)</summary>
        public string? SupplierCatalogNo { get; set; }

        // ── Propiedades dinámicas (QryGroup1..64) ───────────────────────
        /// <summary>
        /// Propiedades del artículo. Property = número (1-64), Value = tYES(-1) / tNO(0).
        /// Equivale a QryGroup{N} en el template de DataTransfer.
        /// </summary>
        public List<SapProperty>? Properties { get; set; }

        // ── Precio (usado en OPLN.Items) ────────────────────────────────
        /// <summary>Precio — usado cuando OITM actúa como línea de OPLN.</summary>
        public double? Price { get; set; }

        // ── Listas de precios ───────────────────────────────────────────
        /// <summary>
        /// Precios por lista al crear el artículo. Cada entrada: { ListNum, Price }.
        /// </summary>
        public List<OPLN>? PriceLists { get; set; }

        // ── Campos de usuario ───────────────────────────────────────────
        /// <summary>UDFs: U_AMARCA, U_AMODELO, U_SubGrupo, U_Proveedor, U_Marcaext, U_Compras</summary>
        public List<UserField>? UserFields { get; set; }
    }
}
