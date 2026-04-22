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

        public ItemMasterDataService UseMovesa()
        {
            _company = SAPConnection.GetMovesaTestCOM();
            return this;
        }

        public ItemMasterDataService UseABCompany()
        {
            _company = SAPConnection.GetABCompanyCOM();
            return this;
        }

        public ApiResponse Connect()
        {
            ApiResponse response = new ApiResponse();
            try
            {
                SAPConnection.Connect();
                return response.Ok();
            }
            catch (Exception ex) { return response.Error(500, ex.Message); }
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

        /// <summary>
        /// Crea un nuevo Artículo (OITM) en SAP B1.
        /// Mapea todos los campos del template de carga masiva.
        /// Si se incluyen PriceLists, asigna el precio en cada lista de precios indicada.
        /// </summary>
        public ApiResponse CreateItem(OITM obj)
        {
            ApiResponse response = new ApiResponse();
            Items item = null;

            try
            {
                item = (Items)_company.GetBusinessObject(BoObjectTypes.oItems);

                // ── Campos básicos ──────────────────────────────────────
                item.ItemCode    = obj.ItemCode;
                item.ItemName    = obj.ItemName;
                if (!string.IsNullOrEmpty(obj.ForeignName))
                    item.ForeignName = obj.ForeignName;

                // ── Clasificación ───────────────────────────────────────
                if (obj.ItemsGroupCode.HasValue)
                    item.ItemsGroupCode = obj.ItemsGroupCode.Value;
                if (obj.CustomsGroupCode.HasValue)
                    item.CustomsGroupCode = obj.CustomsGroupCode.Value;
                if (!string.IsNullOrEmpty(obj.BarCode))
                    item.BarCode = obj.BarCode;

                // ── Flags Y/N ───────────────────────────────────────────
                if (!string.IsNullOrEmpty(obj.VATLiable))
                    item.VatLiable = YN(obj.VATLiable);
                if (!string.IsNullOrEmpty(obj.PurchaseItem))
                    item.PurchaseItem = YN(obj.PurchaseItem);
                if (!string.IsNullOrEmpty(obj.SalesItem))
                    item.SalesItem = YN(obj.SalesItem);
                if (!string.IsNullOrEmpty(obj.InventoryItem))
                    item.InventoryItem = YN(obj.InventoryItem);
                if (!string.IsNullOrEmpty(obj.ManageStockByWarehouse))
                    item.ManageStockByWarehouse = YN(obj.ManageStockByWarehouse);

                // ── Proveedor ───────────────────────────────────────────
                if (!string.IsNullOrEmpty(obj.Mainsupplier))
                    item.Mainsupplier = obj.Mainsupplier;
                if (!string.IsNullOrEmpty(obj.SupplierCatalogNo))
                    item.SupplierCatalogNo = obj.SupplierCatalogNo;

                // ── Propiedades dinámicas (QryGroup1..64) ───────────────
                if (obj.Properties != null)
                {
                    foreach (var prop in obj.Properties)
                    {
                        var pi = item.GetType().GetProperty($"Properties{prop.Property}");
                        if (pi != null && pi.CanWrite)
                            pi.SetValue(item, prop.Value);
                    }
                }

                // ── Listas de precios ───────────────────────────────────
                if (obj.PriceLists != null && obj.PriceLists.Any())
                {
                    foreach (var pl in obj.PriceLists)
                    {
                        if (!pl.ListNum.HasValue || !pl.Price.HasValue) continue;

                        for (int i = 0; i < item.PriceList.Count; i++)
                        {
                            item.PriceList.SetCurrentLine(i);
                            if (item.PriceList.PriceList == pl.ListNum.Value)
                            {
                                item.PriceList.Price = pl.Price.Value;
                                break;
                            }
                        }
                    }
                }

                // ── Campos de usuario (U_*) ─────────────────────────────
                CopyUserFields(obj.UserFields, item.UserFields);

                int ret = item.Add();
                if (ret != 0)
                {
                    _company.GetLastError(out int errCode, out string errMsg);
                    return response.Error(500, $"Error SAP OITM [{obj.ItemCode}]: {errCode} - {errMsg}");
                }

                return response.Ok(new { ItemCode = obj.ItemCode });
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

        #region HELPERS

        private static BoYesNoEnum YN(string? val)
            => val?.Trim().ToUpper() == "Y" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO;

        private static void CopyUserFields(List<UserField>? fields, UserFields target)
        {
            if (fields == null) return;
            foreach (var uf in fields)
            {
                try { target.Fields.Item(uf.Key).Value = uf.Value; }
                catch { }
            }
        }

        #endregion

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
