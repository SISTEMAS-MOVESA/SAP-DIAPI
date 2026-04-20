using IntegracionesSAP.Libs;
using IntegracionesSAP.Models;
using IntegracionesSAP.Models.Purchasing;
using IntegracionesSAP.Services.Purchasing;
using Microsoft.AspNetCore.Mvc;
using SAPbobsCOM;

namespace IntegracionesSAP.Controllers.Purchasing
{
    /// <summary>
    /// Controlador de documentos de compras.
    ///
    /// Flujo de trazabilidad lineal:
    ///   OPRQ (1470000113)
    ///     => OPQT (540000006)   /PurchaseQuotation/CreateItems|CreateServices
    ///     => OPOR (22)          /PurchaseOrder/CreateItems|CreateServices
    ///                             Para copiar desde OPQT: usar BaseEntry+BaseLine+BaseType=540000006 en líneas
    ///     => OPDN (20)          /GoodsReceipt/Create
    ///     => OPCH (18)          /APInvoice/CreateItems|CreateServices
    ///        (OPOR => OPCH también soportado vía BaseType=22 en las líneas)
    /// </summary>
    [Route("v1/Purchase")]
    [ApiController]
    public class PurchaseController : ControllerBase
    {
        private readonly PurchaseTransactionService service;

        public PurchaseController()
        {
            service = new PurchaseTransactionService();
        }

        // ─────────────────────────────────────────────────────────────
        // PURCHASE REQUEST  —  OPRQ  (object type 1470000113)
        // ─────────────────────────────────────────────────────────────

        [HttpPost("PurchaseRequest/Create")]
        public IActionResult CreatePurchaseRequest([FromBody] OPRQ document)
        {
            ApiResponse response = new ApiResponse();
            try
            {
                if (document == null)
                    return Ok(response.Error(400, "Documento no puede ser nulo"));
                if (document.Lines == null || !document.Lines.Any())
                    return Ok(response.Error(400, "El documento no contiene lineas"));

                int n = 0;
                foreach (var l in document.Lines) l.LineNum = n++;

                response = service.Connect();
                if (!response.Success) return Ok(response);
                try
                {
                    response = service.CreatePurchaseRequest(document);
                    EnrichDocNum(response, "OPRQ");
                }
                finally { service.Disconnect(); }
                return Ok(response);
            }
            catch (Exception ex) { return Ok(response.Error(500, ex.Message)); }
        }

        // ─────────────────────────────────────────────────────────────
        // PURCHASE QUOTATION  —  OPQT  (object type 540000006)
        // ─────────────────────────────────────────────────────────────

        /// <summary>Oferta de compra tipo artículos (dDocument_Items).</summary>
        [HttpPost("PurchaseQuotation/CreateItems")]
        public IActionResult CreatePurchaseQuotationItems([FromBody] OPQT document)
        {
            if (document != null) document.DocType = BoDocumentTypes.dDocument_Items;
            return CreatePurchaseQuotationInternal(document);
        }

        /// <summary>Oferta de compra tipo servicios (dDocument_Service).</summary>
        [HttpPost("PurchaseQuotation/CreateServices")]
        public IActionResult CreatePurchaseQuotationServices([FromBody] OPQT document)
        {
            if (document != null) document.DocType = BoDocumentTypes.dDocument_Service;
            return CreatePurchaseQuotationInternal(document);
        }

        private IActionResult CreatePurchaseQuotationInternal(OPQT document)
        {
            ApiResponse response = new ApiResponse();
            try
            {
                if (document == null)
                    return Ok(response.Error(400, "Documento no puede ser nulo"));
                if (document.Lines == null || !document.Lines.Any())
                    return Ok(response.Error(400, "El documento no contiene lineas"));

                int n = 0;
                foreach (var l in document.Lines) l.LineNum = n++;

                response = service.Connect();
                if (!response.Success) return Ok(response);
                try
                {
                    response = service.CreatePurchaseQuotation(document);
                    EnrichDocNum(response, "OPQT");
                }
                finally { service.Disconnect(); }
                return Ok(response);
            }
            catch (Exception ex) { return Ok(response.Error(500, ex.Message)); }
        }

        // ─────────────────────────────────────────────────────────────
        // PURCHASE ORDER  —  OPOR  (object type 22)
        // ─────────────────────────────────────────────────────────────

        /// <summary>Pedido de compra tipo artículos (dDocument_Items).</summary>
        [HttpPost("PurchaseOrder/CreateItems")]
        public IActionResult CreatePurchaseOrderItems([FromBody] OPOR document)
        {
            if (document != null) document.DocType = BoDocumentTypes.dDocument_Items;
            return CreatePurchaseOrderInternal(document);
        }

        /// <summary>Pedido de compra tipo servicios (dDocument_Service).</summary>
        [HttpPost("PurchaseOrder/CreateServices")]
        public IActionResult CreatePurchaseOrderServices([FromBody] OPOR document)
        {
            if (document != null) document.DocType = BoDocumentTypes.dDocument_Service;
            return CreatePurchaseOrderInternal(document);
        }

        private IActionResult CreatePurchaseOrderInternal(OPOR document)
        {
            ApiResponse response = new ApiResponse();
            try
            {
                if (document == null)
                    return Ok(response.Error(400, "Documento no puede ser nulo"));
                if (document.Lines == null || !document.Lines.Any())
                    return Ok(response.Error(400, "El documento no contiene lineas"));

                int n = 0;
                foreach (var l in document.Lines) l.LineNum = n++;

                response = service.Connect();
                if (!response.Success) return Ok(response);
                try
                {
                    response = service.CreatePurchaseOrder(document);
                    EnrichDocNum(response, "OPOR");
                }
                finally { service.Disconnect(); }
                return Ok(response);
            }
            catch (Exception ex) { return Ok(response.Error(500, ex.Message)); }
        }

        // ─────────────────────────────────────────────────────────────
        // GOODS RECEIPT PO  —  OPDN  (object type 20)
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Crea una Entrada de Mercancía de Compras (OPDN).
        /// Para vincular a un OPOR: incluir BaseEntry + BaseLine + BaseType=22 en cada línea.
        /// </summary>
        [HttpPost("GoodsReceipt/Create")]
        public IActionResult CreateGoodsReceipt([FromBody] OPDN document)
        {
            ApiResponse response = new ApiResponse();
            try
            {
                if (document == null)
                    return Ok(response.Error(400, "Documento no puede ser nulo"));
                if (document.Lines == null || !document.Lines.Any())
                    return Ok(response.Error(400, "El documento no contiene lineas"));

                response = service.Connect();
                if (!response.Success) return Ok(response);
                try
                {
                    response = service.CreateGoodsReceipt(document);
                    EnrichDocNum(response, "OPDN");
                }
                finally { service.Disconnect(); }
                return Ok(response);
            }
            catch (Exception ex) { return Ok(response.Error(500, ex.Message)); }
        }

        // ─────────────────────────────────────────────────────────────
        // AP INVOICE  —  OPCH  (object type 18)
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Factura de proveedor tipo artículos (dDocument_Items).
        /// Para copiar desde OPDN: BaseType=20. Para copiar desde OPOR: BaseType=22.
        /// </summary>
        [HttpPost("APInvoice/CreateItems")]
        public IActionResult CreateAPInvoiceItems([FromBody] OPCH document)
        {
            if (document != null) document.DocType = BoDocumentTypes.dDocument_Items;
            return CreateAPInvoiceInternal(document);
        }

        /// <summary>
        /// Factura de proveedor tipo servicios (dDocument_Service).
        /// Para copiar desde OPOR de servicios: BaseType=22.
        /// </summary>
        [HttpPost("APInvoice/CreateServices")]
        public IActionResult CreateAPInvoiceServices([FromBody] OPCH document)
        {
            if (document != null) document.DocType = BoDocumentTypes.dDocument_Service;
            return CreateAPInvoiceInternal(document);
        }

        private IActionResult CreateAPInvoiceInternal(OPCH document)
        {
            ApiResponse response = new ApiResponse();
            try
            {
                if (document == null)
                    return Ok(response.Error(400, "Documento no puede ser nulo"));
                if (document.Lines == null || !document.Lines.Any())
                    return Ok(response.Error(400, "El documento no contiene lineas"));

                int n = 0;
                foreach (var l in document.Lines) l.LineNum = n++;

                response = service.Connect();
                if (!response.Success) return Ok(response);
                try
                {
                    response = service.CreateAPInvoice(document);
                    EnrichDocNum(response, "OPCH");
                }
                finally { service.Disconnect(); }
                return Ok(response);
            }
            catch (Exception ex) { return Ok(response.Error(500, ex.Message)); }
        }

        // ─────────────────────────────────────────────────────────────
        // HELPER
        // ─────────────────────────────────────────────────────────────

        private static void EnrichDocNum(ApiResponse response, string table)
        {
            if (response.Success && response.Data is SAPObjResult result)
                result.DocNum = SAPConnection.GetObjDocNum(table, result.DocEntry ?? 0);
        }
    }
}
