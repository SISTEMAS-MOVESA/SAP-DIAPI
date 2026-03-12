using IntegracionesSAP.Libs;
using IntegracionesSAP.Models;
using IntegracionesSAP.Models.Inventory;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace IntegracionesSAP.Controllers
{
    [Route("v1/Info")]
    [ApiController]
    public class MainController : ControllerBase
    {
        [HttpGet("")]
        public IActionResult ApiInfo()
        {
            ApiResponse response = new ApiResponse();
            try
            {
                //var db = MSSQL.DB_DEFAULT;
                var con = SAPConnection.GetDefaultCOM();
                response.Ok(new
                {
                    Server = con.Server,
                    CompanyDB = con.CompanyDB,
                    UserName = con.UserName,
                    LicenseServer = con.LicenseServer,
                    SLDServer = con.SLDServer
                });

                return Ok(response);
            }
            catch (Exception ex)
            {
                return Ok(response.Error(500, ex.Message));
            }
        }
    }
}
