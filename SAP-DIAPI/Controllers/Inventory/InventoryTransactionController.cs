using Azure;
using Azure.Core;
using IntegracionesSAP.Libs;
using IntegracionesSAP.Models;
using IntegracionesSAP.Models.Inventory;
using IntegracionesSAP.Services.Inventory;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace IntegracionesSAP.Controllers.Inventory
{
    [Route("test/InventoryTransaction")]
    [ApiController]
    public class InventoryTransactionController : ControllerBase
    {
        private readonly InventoryTransactionService service;

        public InventoryTransactionController()
        {
            service = new InventoryTransactionService();
        }

        [HttpPost("TransferRequest/Create")]
        public IActionResult CreateTransferRequest([FromBody] OWTQ document)
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
                    response = service.CreateTransferRequest(document);
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

        [HttpPost("StockTransfer/Create")]
        public IActionResult CreateStockTransfer([FromBody] OWTR document)
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
                    response = service.CreateStockTransfer(document);
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

        [HttpPost("DoTransferFlow")]
        public IActionResult DoTransferFlow([FromBody] OWTQ document)
        {
            ApiResponse response = new ApiResponse();
            try
            {
                if (document == null)
                    return Ok(response.Error(400, "Documento no puede ser nulo"));

                if (document.Lines == null || document.Lines.Count == 0)
                    return Ok(response.Error(400, "El documento no contiene lineas"));

                if (document.TargetSeries == null)
                    return Ok(response.Error(400, "[TargetSeries] no puede ser nulo"));

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
                    response = service.CreateTransferFlow(document);
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
