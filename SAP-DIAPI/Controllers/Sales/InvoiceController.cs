using IntegracionesSAP.Models;
using IntegracionesSAP.Models.Sales;
using IntegracionesSAP.Services.Sales;
using Microsoft.AspNetCore.Mvc;

namespace IntegracionesSAP.Controllers.Sales
{
    [Route("v1/Sales/Invoice")]
    [ApiController]
    public class InvoiceController : ControllerBase
    {
        private readonly SalesTransactionService service;

        public InvoiceController()
        {
            service = new SalesTransactionService();
        }

        /// <summary>
        /// Anula facturas de reserva masivamente.
        /// Body: { "DocEntries": [1001, 1002, 1003], "Comments": "Motivo de anulación" }
        /// </summary>
        [HttpPost("Reserve/Cancel/Bulk")]
        public IActionResult CancelReserveInvoicesBulk([FromBody] CancelBulkRequest request)
        {
            ApiResponse response = new ApiResponse();
            try
            {
                if (request == null || request.DocEntries == null || !request.DocEntries.Any())
                    return Ok(response.Error(400, "La lista [DocEntries] no puede estar vacía"));

                if (string.IsNullOrWhiteSpace(request.Comments))
                    return Ok(response.Error(400, "Campo [Comments] no puede estar vacío"));

                response = service.Connect();
                if (!response.Success) return Ok(response);

                try
                {
                    response = service.CancelReserveInvoicesBulk(request.DocEntries, request.Comments);
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
