using System.Runtime.InteropServices;
using IntegracionesSAP.Libs;
using IntegracionesSAP.Models;
using IntegracionesSAP.Models.Inventory;
using SAPbobsCOM;

namespace IntegracionesSAP.Services.Inventory
{
    public class PriceListService
    {
        private readonly Company _company;

        public PriceListService()
        {
            _company = SAPConnection.GetMovesaCOM();
        }

        public ApiResponse Connect()
        {
            ApiResponse response = new ApiResponse();
            try
            {
                SAPConnection.Connect();
                return response.Ok(null, "Conectado a SAP");
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
                return response.Ok(null, "Desconectado de SAP");
            }
            catch (Exception ex)
            {
                return response.Error(500, ex.Message);
            }
        }

        /// <summary>
        /// Actualiza el precio de una lista de precios (OPLN) para múltiples artículos.
        /// Itera sobre cada OITM, localiza la lista por ListNum y sobreescribe el precio.
        /// </summary>
        public ApiResponse UpdatePriceList(OPLN model)
        {
            ApiResponse response = new ApiResponse();
            Items? item = null;

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
                            item.PriceList.Price = (double)line.Price!;
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

                    Marshal.ReleaseComObject(item);
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
                if (item != null) Marshal.ReleaseComObject(item);
            }
        }

        /// <summary>
        /// Upsert de precios especiales para un socio específico (OSPP).
        /// Si CardCode+ItemCode ya existe lo actualiza, si no lo crea.
        /// </summary>
        public ApiResponse UpdateSpecialPrices(OSPP model)
        {
            ApiResponse response = new ApiResponse();
            SpecialPrices? sp = null;

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

                    sp.Price = (double)item.Price!;

                    int ret = exists ? sp.Update() : sp.Add();

                    if (ret != 0)
                    {
                        _company.GetLastError(out int errCode, out string errMsg);
                        return response.Error(500, $"Error SAP OSPP: {errCode} - {errMsg}");
                    }

                    Marshal.ReleaseComObject(sp);
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
                if (sp != null) Marshal.ReleaseComObject(sp);
            }
        }

        /// <summary>
        /// Upsert masivo de precios especiales: misma lista de ítems aplicada a todos los socios.
        /// Si CardCode+ItemCode ya existe lo actualiza, si no lo crea.
        /// </summary>
        public ApiResponse UploadSpecialPrices(OSPP model)
        {
            ApiResponse response = new ApiResponse();
            var results = new List<object>();

            string currency = string.IsNullOrWhiteSpace(model.Currency) ? "LPS" : model.Currency;

            SpecialPrices? sp = null;

            try
            {
                foreach (var customer in model.CardCodes)
                {
                    string cardCode = customer.CardCode ?? "";
                    if (string.IsNullOrWhiteSpace(cardCode)) continue;

                    int saved  = 0;
                    var errors = new List<string>();

                    foreach (var item in model.Items)
                    {
                        if (string.IsNullOrEmpty(item.ItemCode)) continue;

                        sp = (SpecialPrices)_company.GetBusinessObject(BoObjectTypes.oSpecialPrices);

                        bool exists = sp.GetByKey(item.ItemCode, cardCode);

                        if (!exists)
                        {
                            sp.CardCode = cardCode;
                            sp.ItemCode = item.ItemCode;
                            sp.Currency = currency;
                        }

                        sp.Price = (double)(item.Price ?? 0);

                        int ret = exists ? sp.Update() : sp.Add();

                        if (ret != 0)
                        {
                            _company.GetLastError(out int errCode, out string errMsg);
                            errors.Add($"{item.ItemCode}: {errCode} - {errMsg}");
                        }
                        else
                        {
                            saved++;
                        }

                        Marshal.ReleaseComObject(sp);
                        sp = null;
                    }

                    results.Add(new
                    {
                        CardCode = cardCode,
                        Saved    = saved,
                        Errors   = errors
                    });
                }

                return response.Ok(results, $"{results.Count} socio(s) procesados.");
            }
            catch (Exception ex)
            {
                return response.Error(500, ex.Message);
            }
            finally
            {
                if (sp != null) Marshal.ReleaseComObject(sp);
            }
        }
    }
}
