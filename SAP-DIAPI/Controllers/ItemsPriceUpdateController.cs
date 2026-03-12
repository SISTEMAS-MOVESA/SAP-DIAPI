using IntegracionesSAP;
using Microsoft.AspNetCore.Mvc;

namespace IntegracionesSAP.Controllers
{
    [ApiController]
    [Route("test/[controller]")]
    public class ItemsPriceUpdateController : ControllerBase
    {
        /// <summary>
        /// Actualiza precios de artículos en batch.
        /// </summary>
        [HttpPut]
        public IActionResult UpdatePrices([FromBody] ItemPriceUpdateBatch batch)
        {
            if (batch == null || batch.Items.Count == 0)
                return BadRequest("No se enviaron datos para actualizar.");

            try
            {
                SapDiApi.Connect();
                SapDiApi.UpdateItemPriceBatch(batch);

                return Ok(new
                {
                    success = true,
                    totalProcesados = batch.Items.Count,
                    resultados = batch.Items
                });
            }
            catch (Exception ex)
            {
                return Problem(
                    detail: ex.Message,
                    statusCode: 500,
                    title: "Error inesperado en ItemsPriceUpdateController"
                );
            }
        }
    }
}
