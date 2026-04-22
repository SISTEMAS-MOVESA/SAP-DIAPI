using IntegracionesSAP.Models;
using IntegracionesSAP.Models.Inventory;
using IntegracionesSAP.Services.Inventory;
using Microsoft.AspNetCore.Mvc;

namespace IntegracionesSAP.Controllers.Inventory
{
    /// <summary>
    /// Controlador para gestión de maestro de artículos (OITM).
    ///
    /// Endpoints:
    ///   POST /v1/Inventory/ItemMasterData/Create/Movesa    — Crea artículo en MOVESA
    ///   POST /v1/Inventory/ItemMasterData/Create/ABCompany — Crea artículo en ABCOMPANY
    /// </summary>
    [Route("v1/Inventory/ItemMasterData")]
    [ApiController]
    public class ItemMasterDataController : ControllerBase
    {
        private readonly ItemMasterDataService service;

        public ItemMasterDataController()
        {
            service = new ItemMasterDataService();
        }

        [HttpPost("Create/Movesa")]
        public IActionResult CreateItemMovesa([FromBody] OITM document)
            => CreateItemInternal(document, () => service.UseMovesa().Connect());

        [HttpPost("Create/ABCompany")]
        public IActionResult CreateItemABCompany([FromBody] OITM document)
            => CreateItemInternal(document, () => service.UseABCompany().Connect());

        // ─────────────────────────────────────────────────────────────
        // INTERNO
        // ─────────────────────────────────────────────────────────────

        private IActionResult CreateItemInternal(OITM document, Func<ApiResponse> connectFn)
        {
            ApiResponse response = new ApiResponse();
            try
            {
                if (document == null)
                    return Ok(response.Error(400, "El artículo no puede ser nulo"));
                if (string.IsNullOrWhiteSpace(document.ItemCode))
                    return Ok(response.Error(400, "ItemCode es requerido"));
                if (string.IsNullOrWhiteSpace(document.ItemName))
                    return Ok(response.Error(400, "ItemName es requerido"));

                response = connectFn();
                if (!response.Success) return Ok(response);
                try
                {
                    response = service.CreateItem(document);
                }
                finally { service.Disconnect(); }
                return Ok(response);
            }
            catch (Exception ex) { return Ok(response.Error(500, ex.Message)); }
        }
    }
}
