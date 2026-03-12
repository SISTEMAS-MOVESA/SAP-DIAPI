using Microsoft.AspNetCore.Mvc;
using IntegracionesSAP;

namespace IntegracionesSAP.Controllers
{
    [ApiController]
    [Route("test/[controller]")]
    public class SalesOrderController : ControllerBase
    {
        /// <summary>
        /// Recibe un pedido de venta en formato JSON y lo envía a SAP B1.
        /// </summary>
        [HttpPost("create")]
        public IActionResult CreateSalesOrder([FromBody] OrderHeader order)
        {
            if (order == null)
            {
                return BadRequest(new { message = "No se recibió el objeto de orden." });
            }

            if (string.IsNullOrWhiteSpace(order.CardCode))
            {
                return BadRequest(new { message = "El campo 'CardCode' es obligatorio." });
            }

            if (order.Lines == null || !order.Lines.Any())
            {
                return BadRequest(new { message = "Debe incluir al menos una línea en la orden." });
            }

            try
            {
                SapDiApi.Connect();
                int docEntry = SapDiApi.CreateOrder(order);

                return Ok(new
                {
                    message = "Orden creada correctamente en SAP B1",
                    DocEntry = docEntry
                });

            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Error al crear la orden en SAP B1",
                    error = ex.Message
                });
            }
            finally
            {
                SapDiApi.Disconnect();
            }
        }

        /// <summary>
        /// Actualizar Precio de una Orden de Venta SAP B1.
        /// </summary>
        [HttpPost("UpdateSalesOrderLinePrice")]
        public IActionResult UpdateSalesOrderLinePrice([FromBody] OrderHeader order)
        {
            if (order == null)
            {
                return BadRequest(new { message = "No se recibió el objeto de orden." });
            }

            if (order.DocEntry <= 0)
            {
                return BadRequest(new { message = "Docentry Debe ser Igual o Mayor que Cero" });
            }

            if (order.Lines == null || !order.Lines.Any())
            {
                return BadRequest(new { message = "Debe incluir al menos una línea en la orden." });
            }

            try
            {
                SapDiApi.Connect();

                int docEntry = SapDiApi.UpdateLinePrice(order);

                return Ok(new
                {
                    message = "Precios Actualizados Correctamente en SAP B1",
                    DocEntry = docEntry
                });

            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Error al Actualizar la Orden en SAP B1",
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
