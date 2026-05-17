using IntegracionesSAP.Libs;
using IntegracionesSAP.Models;
using IntegracionesSAP.Models.Inventory;
using IntegracionesSAP.Models.Sales;
using IntegracionesSAP.Services.Inventory;
using IntegracionesSAP.Services.Sales;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace IntegracionesSAP.Controllers.Sales
{
    [Route("v1/Sales/SalesOrder")]
    [ApiController]
    public class SalesOrderController : ControllerBase
    {
        private readonly SalesTransactionService service;

        public SalesOrderController()
        {
            service = new SalesTransactionService();
        }

        [HttpPost("Create")]
        public IActionResult CreateSalesOrder([FromBody] ORDR document)
        {
            ApiResponse response = new ApiResponse();
            try
            {
                if (document == null)
                    return Ok(response.Error(400, "Documento no puede ser nulo"));

                if (document.Lines == null || !document.Lines.Any())
                    return Ok(response.Error(400, "El documento no contiene lineas"));

                int LineNum = 0;
                foreach (var item in document.Lines)
                {
                    item.LineNum = LineNum;
                    LineNum++;
                }

                response = service.Connect();
                if (!response.Success) return Ok(response);

                try
                {
                    response = service.CreateSalesOrder(document);
                    if (response.Success && response.Data is SAPObjResult result)
                    {
                        result.DocNum = SAPConnection.GetObjDocNum("ORDR", result.DocEntry ?? 0);
                    }
                }
                finally
                {
                    service.Disconnect();
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                return Ok(response.Error(500, ex.Message));
            }
        }

        [HttpPost("CopyToDelivery")]
        public IActionResult OrderToDelivery([FromBody] ORDR document)
        {
            ApiResponse response = new ApiResponse();
            try
            {
                if (document == null)
                    return Ok(response.Error(400, "Documento no puede ser nulo"));
                if (document.DocEntry == null)
                    return Ok(response.Error(400, "Campo [DocEntry] no pueder ser nulo."));
                if (document.TargetSeries == 0)
                    return Ok(response.Error(400, "Campo [TargetSeries] ingresado es invalido."));

                response = service.Connect();
                if (!response.Success) return Ok(response);

                try
                {
                    response = service.CopyOrderToDelivery(document);
                    if (response.Success && response.Data is SAPObjResult result)
                    {
                        result.DocNum = SAPConnection.GetObjDocNum("ODLN", result.DocEntry ?? 0);
                    }
                }
                finally
                {
                    service.Disconnect();
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                return Ok(response.Error(500, ex.Message));
            }
        }

        [HttpPost("CopyToInvoice")]
        public IActionResult OrderToInvoice([FromBody] ORDR document)
        {
            ApiResponse response = new ApiResponse();
            try
            {
                if (document == null)
                    return Ok(response.Error(400, "Documento no puede ser nulo"));
                if (document.DocEntry == null)
                    return Ok(response.Error(400, "Campo [DocEntry] no pueder ser nulo."));
                if (document.TargetSeries == 0)
                    return Ok(response.Error(400, "Campo [TargetSeries] ingresado es invalido."));

                response = service.Connect();
                if (!response.Success) return Ok(response);

                try
                {
                    response = service.CopyOrderToInvoice(document);
                    if (response.Success && response.Data is SAPObjResult result)
                    {
                        result.DocNum = SAPConnection.GetObjDocNum("OINV", result.DocEntry ?? 0);
                    }
                }
                finally
                {
                    service.Disconnect();
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                return Ok(response.Error(500, ex.Message));
            }
        }

        [HttpPost("Cancel")]
        public IActionResult CancelSalesOrder([FromBody] ORDR document)
        {
            ApiResponse response = new ApiResponse();
            try
            {
                if (document.DocEntry == null || document.DocEntry == 0)
                    return Ok(response.Error(500, "Campo [DocEntry] no puede estar vacio."));

                if (string.IsNullOrEmpty(document.Comments))
                    return Ok(response.Error(500, "Campo [Comments] no puede estar vacio."));

                response = service.Connect();
                if (!response.Success) return Ok(response);

                try
                {
                    response = service.CancelSalesOrder(document);
                }
                finally
                {
                    service.Disconnect();
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                return Ok(response.Error(500, ex.Message));
            }
        }

        [HttpPost("Close")]
        public IActionResult CloseSalesOrder([FromBody] ORDR document)
        {
            ApiResponse response = new ApiResponse();
            try
            {
                if (document.DocEntry == null || document.DocEntry == 0)
                    return Ok(response.Error(500, "Campo [DocEntry] no puede estar vacio."));

                if (string.IsNullOrEmpty(document.Comments))
                    return Ok(response.Error(500, "Campo [Comments] no puede estar vacio."));

                response = service.Connect();
                if (!response.Success) return Ok(response);

                try
                {
                    response = service.CloseSalesOrder(document);
                }
                finally
                {
                    service.Disconnect();
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                return Ok(response.Error(500, ex.Message));
            }
        }

        [HttpPost("Update/Header")]
        public IActionResult UpdateSalesOrderHeader([FromBody] ORDR document)
        {
            ApiResponse response = new ApiResponse();
            try
            {
                if (document?.DocEntry == null || document.DocEntry == 0)
                    return Ok(response.Error(400, "Campo [DocEntry] es requerido."));

                if (document.DiscPrcnt == null && document.DocTotal == null && string.IsNullOrEmpty(document.Comments))
                    return Ok(response.Error(400, "Debe especificar al menos un campo a actualizar (DiscPrcnt, DocTotal o Comments)."));

                response = service.Connect();
                if (!response.Success) return Ok(response);

                try { response = service.UpdateSalesOrderHeader(document); }
                finally { service.Disconnect(); }

                return Ok(response);
            }
            catch (Exception ex)
            {
                return Ok(response.Error(500, ex.Message));
            }
        }

        [HttpPost("Update/Line")]
        public IActionResult UpdateSalesOrderLine([FromBody] ORDR document)
        {
            ApiResponse response = new ApiResponse();
            try
            {
                if (document?.DocEntry == null || document.DocEntry == 0)
                    return Ok(response.Error(400, "Campo [DocEntry] es requerido."));

                if (document.Lines == null || !document.Lines.Any())
                    return Ok(response.Error(400, "Debe especificar al menos una línea a actualizar."));

                response = service.Connect();
                if (!response.Success) return Ok(response);

                try { response = service.UpdateSalesOrderLine(document); }
                finally { service.Disconnect(); }

                return Ok(response);
            }
            catch (Exception ex)
            {
                return Ok(response.Error(500, ex.Message));
            }
        }

        [HttpPost("Create/Line")]
        public IActionResult CreateSalesOrderLine([FromBody] ORDR document)
        {
            ApiResponse response = new ApiResponse();
            try
            {
                if (document?.DocEntry == null || document.DocEntry == 0)
                    return Ok(response.Error(400, "Campo [DocEntry] es requerido."));

                if (document.Lines == null || !document.Lines.Any())
                    return Ok(response.Error(400, "Debe especificar al menos una línea a agregar."));

                response = service.Connect();
                if (!response.Success) return Ok(response);

                try { response = service.CreateSalesOrderLine(document); }
                finally { service.Disconnect(); }

                return Ok(response);
            }
            catch (Exception ex)
            {
                return Ok(response.Error(500, ex.Message));
            }
        }

[HttpGet("Invoice/PDF/{docEntry}")]
        public IActionResult GetInvoicePdf(int docEntry)
        {
            ApiResponse response = new ApiResponse();
            try
            {

                response = service.Connect();
                if (!response.Success) return Ok(response);

                try
                {
                    var pdf = service.GenerateInvoicePdf(docEntry);
                    string base64 = Convert.ToBase64String(pdf);
                    response.Ok(new
                    {
                        DocEntry = docEntry,
                        PdfBase64 = base64
                    });
                }
                finally
                {
                    service.Disconnect();
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                return Ok(response.Error(500, ex.Message));
            }
        }
    }
}
