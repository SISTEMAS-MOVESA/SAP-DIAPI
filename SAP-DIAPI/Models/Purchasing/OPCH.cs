using SAPbobsCOM;

namespace IntegracionesSAP.Models.Purchasing
{
    /// <summary>
    /// Factura de Proveedores (AP Invoice) — SAP object type 18
    /// Puede generarse como documento independiente o copiando desde
    /// una Entrada de Mercancía (OPDN, BaseType=20) o un Pedido de Compra (OPOR, BaseType=22).
    /// </summary>
    public class OPCH
    {
        public string? CardCode { get; set; }
        public int? Series { get; set; }
        public DateTime DocDate { get; set; } = DateTime.Now;
        public DateTime DocDueDate { get; set; } = DateTime.Now;
        public string DocCurrency { get; set; } = "USD";
        public BoDocumentTypes DocType { get; set; } = BoDocumentTypes.dDocument_Items;
        public string? Comments { get; set; }
        public string? NumAtCard { get; set; }
        public List<UserField> UserFields { get; set; } = new();
        public List<PCH1> Lines { get; set; } = new();
    }
}
