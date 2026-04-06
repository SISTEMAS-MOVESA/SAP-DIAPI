using IntegracionesSAP.Libs;
using IntegracionesSAP.Models;
using IntegracionesSAP.Models.Purchasing;
using IntegracionesSAP.Models.Sales;
using IntegracionesSAP.Services.Purchasing;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace IntegracionesSAP.Controllers.Purchasing
{
    [Route("v1/Purchase")]
    [ApiController]
    public class PurchaseController : ControllerBase
    {
        private readonly PurchaseTransactionService service;

        public PurchaseController()
        {
            service = new PurchaseTransactionService();
        }

        [HttpPost("PurchaseOrder/Create")]
        public IActionResult CreatePurchaseOrder([FromBody] OPOR document)
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
                    response = service.CreatePurchaseOrder(document);
                    if (response.Success && response.Data is SAPObjResult result)
                    {
                        result.DocNum = SAPConnection.GetObjDocNum("OPOR", result.DocEntry ?? 0);
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

    }
}
