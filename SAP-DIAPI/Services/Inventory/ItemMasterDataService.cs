using IntegracionesSAP.Libs;
using IntegracionesSAP.Models;
using IntegracionesSAP.Models.Inventory;
using SAPbobsCOM;

namespace IntegracionesSAP.Services.Inventory
{
    public class ItemMasterDataService
    {
        private Company _company;

        public ItemMasterDataService()
        {
            _company = SAPConnection.GetDefaultCOM();
        }

        public ApiResponse Connect()
        {
            ApiResponse response = new ApiResponse();
            try
            {
                SAPConnection.Connect();
                return response.Ok();
            }
            catch (Exception ex)
            {
                return response.Error(500, ex.Message);
            }
        }

        public ApiResponse Disconnect()
        {
            ApiResponse response = new ApiResponse();
            try
            {
                SAPConnection.Disconnect();
                return response.Ok();
            }
            catch (Exception ex)
            {
                return response.Error(500, ex.Message);
            }
        }

        public ApiResponse UpdatePriceList(OPLN model)
        {
            ApiResponse response = new ApiResponse();
            Items item = null;

            try
            {
                foreach (var line in model.Items)
                {
                    item = (Items)_company.GetBusinessObject(BoObjectTypes.oItems);

                    if (!item.GetByKey(line.ItemCode))
                        return response.Error(404, $"Artículo [{line.ItemCode}] no encontrado");

                    bool found = false;

                    for (int i = 0; i < item.PriceList.Count; i++)
                    {
                        item.PriceList.SetCurrentLine(i);

                        if (item.PriceList.PriceList == model.ListNum)
                        {
                            item.PriceList.Price = (double)line.Price;
                            found = true;
                            break;
                        }
                    }

                    if (!found)
                        return response.Error(400, $"Lista [{model.ListNum}] no existe para el artículo {line.ItemCode}");

                    int ret = item.Update();

                    if (ret != 0)
                    {
                        _company.GetLastError(out int errCode, out string errMsg);
                        return response.Error(500, $"Error SAP OITM: {errCode} - {errMsg}");
                    }

                    System.Runtime.InteropServices.Marshal.ReleaseComObject(item);
                    item = null;
                }

                return response.Ok(null, $"Precios actualizados para lista {model.ListNum}");
            }
            catch (Exception ex)
            {
                return response.Error(500, ex.Message);
            }
            finally
            {
                if (item != null)
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(item);
            }
        }

        public ApiResponse UpdateSpecialPrices(OSPP model)
        {
            ApiResponse response = new ApiResponse();
            SpecialPrices sp = null;

            try
            {
                foreach (var item in model.Items)
                {
                    sp = (SpecialPrices)_company.GetBusinessObject(BoObjectTypes.oSpecialPrices);

                    bool exists = sp.GetByKey(item.ItemCode, model.CardCode);

                    if (!exists)
                    {
                        sp.CardCode = model.CardCode;
                        sp.ItemCode = item.ItemCode;
                    }

                    sp.Price = (double)item.Price;

                    int ret = exists ? sp.Update() : sp.Add();

                    if (ret != 0)
                    {
                        _company.GetLastError(out int errCode, out string errMsg);
                        return response.Error(500, $"Error SAP OSPP: {errCode} - {errMsg}");
                    }

                    System.Runtime.InteropServices.Marshal.ReleaseComObject(sp);
                    sp = null;
                }

                return response.Ok(null, "Precios especiales actualizados correctamente");
            }
            catch (Exception ex)
            {
                return response.Error(500, ex.Message);
            }
            finally
            {
                if (sp != null)
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(sp);
            }
        }
    }
}
