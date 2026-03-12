using Microsoft.AspNetCore.Mvc;
using IntegracionesSAP;
using SAPbobsCOM;

namespace IntegracionesSAP.Controllers
{
    [ApiController]
    [Route("test/[controller]")]
    public class DeliveriesController : ControllerBase
    {
        /// <summary>
        /// Convierte una Orden de Venta en una Entrega (Delivery) en SAP B1.
        /// </summary>
        [HttpPost("ConvertOrderToDelivery")]
        public IActionResult CreateDeliveryFromOrder([FromBody] OrderHeader order)
        {
            if (order.DocEntry <= 0)
            {
                return BadRequest(new { message = "Se Requiere un Docentry Valido!" });
            }
            if (order.DocSeries <= 0)
            {
                return BadRequest(new { message = "Se Requiere una Serie de Documento Valida!" });
            }

            try
            {
                SapDiApi.Connect();
                int deliveryDocEntry = SapDiApi.CreateDeliveryFromOrder(order);

                return Ok(new
                {
                    message = "Entrega creada correctamente en SAP B1",
                    DeliveryDocEntry = deliveryDocEntry
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Error al crear la entrega en SAP B1",
                    error = ex.Message
                });
            }
            finally
            {
                SapDiApi.Disconnect();
            }
        }
    }
}
