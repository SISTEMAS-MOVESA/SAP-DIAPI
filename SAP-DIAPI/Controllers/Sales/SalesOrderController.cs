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

        [HttpPost("Cancel/{DocEntry}")]
        public IActionResult CancelSalesOrder([FromRoute] int DocEntry)
        {
            ApiResponse response = new ApiResponse();
            try
            {
                if (DocEntry == null || DocEntry == 0)
                    return Ok(response.Error(500, "Campo [DocEntry] no puede estar vacio."));

                response = service.Connect();
                if (!response.Success) return Ok(response);

                try
                {
                    response = service.CancelSalesOrder(DocEntry);
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

        [HttpPost("Close/{DocEntry}")]
        public IActionResult CloseSalesOrder([FromRoute] int DocEntry)
        {
            ApiResponse response = new ApiResponse();
            try
            {
                if (DocEntry == null || DocEntry == 0)
                    return Ok(response.Error(500, "Campo [DocEntry] no puede estar vacio."));

                response = service.Connect();
                if (!response.Success) return Ok(response);

                try
                {
                    response = service.CloseSalesOrder(DocEntry);
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
