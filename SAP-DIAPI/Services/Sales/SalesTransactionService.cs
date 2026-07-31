using IntegracionesSAP.Libs;
using IntegracionesSAP.Models;
using IntegracionesSAP.Models.Sales;
using Microsoft.IdentityModel.Tokens;
using SAPbobsCOM;

namespace IntegracionesSAP.Services.Sales
{
    public class SalesTransactionService
    {
        private Company _company;

        public SalesTransactionService()
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

        // -- orden de venta
        public ApiResponse CreateSalesOrder(ORDR obj)
        {
            ApiResponse response = new ApiResponse();
            Documents SAPobj = null;

            try
            {
                SAPobj = (Documents)_company.GetBusinessObject(BoObjectTypes.oOrders);

                SAPobj.CardCode = obj.CardCode;
                if (!string.IsNullOrEmpty(obj.CardName)) SAPobj.CardName = obj.CardName;
                if (!string.IsNullOrEmpty(obj.NumAtCard)) SAPobj.NumAtCard = obj.NumAtCard;

                SAPobj.DocDate = obj.DocDate;
                SAPobj.TaxDate = obj.TaxDate;
                SAPobj.DocDueDate = obj.DocDueDate;

                if (obj.PaymentGroupCode != null) SAPobj.PaymentGroupCode = (int)obj.PaymentGroupCode;
                if (obj.DocTotal != null) SAPobj.DocTotal = (double)obj.DocTotal;
                
                SAPobj.Series = obj.Series;
                SAPobj.Comments = obj.Comments;
                SAPobj.SalesPersonCode = obj.SalesPersonCode;


                foreach (var UF in obj.UserFields ?? [])
                {
                    try
                    {
                        SAPobj.UserFields.Fields.Item(UF.Key).Value = UF.Value;
                    }
                    catch { }
                }

                foreach (var item in obj.Lines)
                {
                    SAPobj.Lines.SetCurrentLine((int)item.LineNum);

                    SAPobj.Lines.ItemCode = item.ItemCode;
                    SAPobj.Lines.Quantity = item.Quantity;
                    SAPobj.Lines.WarehouseCode = item.WhsCode;

                    if (item.Price != null) SAPobj.Lines.Price = (double)item.Price;
                    if (item.TaxCode != null) SAPobj.Lines.TaxCode = item.TaxCode;
                    if (item.DiscountPercent != null) SAPobj.Lines.DiscountPercent = (double)item.DiscountPercent;
                    //SAPobj.Lines.DiscountPercent = (double)item.DiscountPercent;

                    if (!string.IsNullOrEmpty(item.SerialNumber))
                    {
                        SAPobj.Lines.UserFields.Fields.Item("U_MSERIE").Value = item.SerialNumber;
                        SAPobj.Lines.SerialNum = item.SerialNumber;

                        if (item.SysSerial != null)
                        {
                            SAPobj.Lines.SerialNumbers.SystemSerialNumber = (int)item.SysSerial;
                            SAPobj.Lines.SerialNumbers.Add();
                        }
                    }

                    // campos de usuario - linea
                    foreach (var UF in item.UserFields ?? [])
                    {
                        try
                        {
                            SAPobj.Lines.UserFields.Fields.Item(UF.Key).Value = UF.Value;
                        }
                        catch { }
                    }

                    SAPobj.Lines.Add();
                }

                int ret = SAPobj.Add();

                if (ret != 0)
                {
                    _company.GetLastError(out int errCode, out string errMsg);
                    return response.Error(500, $"Error SAP ORDR: {errCode} - {errMsg}");
                }

                int docEntry = int.Parse(_company.GetNewObjectKey());

                return response.Ok(new SAPObjResult
                {
                    DocEntry = docEntry, DocType = BoObjectTypes.oOrders
                });
            }
            catch (Exception ex)
            {
                return response.Error(500, $"Error SAP ORDR: {ex.Message}");
            }
            finally
            {
                if (SAPobj != null)
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(SAPobj);
            }
        }

        public ApiResponse CloseSalesOrder(ORDR obj)
        {
            ApiResponse response = new ApiResponse();
            Documents BaseDoc = null;

            try
            {
                BaseDoc = (Documents)_company.GetBusinessObject(BoObjectTypes.oOrders);

                if (!BaseDoc.GetByKey((int)obj.DocEntry))
                    return response.Error(404, $"Orden [{obj.DocEntry}] no encontrada");

                for (int i = 0; i < BaseDoc.Lines.Count; i++)
                {
                    BaseDoc.Lines.SetCurrentLine(i);

                    if (BaseDoc.Lines.LineStatus == BoStatus.bost_Open)
                    {
                        BaseDoc.Lines.LineStatus = BoStatus.bost_Close;
                    }
                }

                BaseDoc.Comments = obj.Comments;
                int ret = BaseDoc.Update();

                if (ret != 0)
                {
                    _company.GetLastError(out int errCode, out string errMsg);
                    return response.Error(500, $"Error closing ORDR: {errCode} - {errMsg}");
                }

                return response.Ok(BaseDoc.DocEntry, "Orden cerrada correctamente");
            }
            catch (Exception ex)
            {
                return response.Error(500, ex.Message);
            }
            finally
            {
                if (BaseDoc != null)
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(BaseDoc);
            }
        }

        public ApiResponse CancelSalesOrder(ORDR obj)
        {
            ApiResponse response = new ApiResponse();
            Documents SAPobj = null;
            int ret;
            try
            {
                SAPobj = (Documents)_company.GetBusinessObject(BoObjectTypes.oOrders);

                if (!SAPobj.GetByKey((int)obj.DocEntry))
                    return response.Error(404, $"Orden [{obj.DocEntry}] no encontrada");

                // actualizar comentarios
                SAPobj.Comments = obj.Comments;
                ret = SAPobj.Update();
                if (ret != 0)
                {
                    _company.GetLastError(out int errCode, out string errMsg);
                    return response.Error(500, $"Error cancelling ORDR: {errCode} - {errMsg}");
                }

                // cancelar
                ret = SAPobj.Cancel();
                if (ret != 0)
                {
                    _company.GetLastError(out int errCode, out string errMsg);
                    return response.Error(500, $"Error cancelling ORDR: {errCode} - {errMsg}");
                }

                return response.Ok(obj.DocEntry, "Orden cancelada correctamente");
            }
            catch (Exception ex)
            {
                return response.Error(500, ex.Message);
            }
            finally
            {
                if (SAPobj != null)
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(SAPobj);
            }
        }

        public ApiResponse CopyOrderToDelivery(ORDR obj)
        {
            ApiResponse response = new ApiResponse();
            Documents SAPobj = null;

            try
            {
                SAPobj = (Documents)_company.GetBusinessObject(BoObjectTypes.oDeliveryNotes);

                SAPobj.DocDate = DateTime.Now;
                SAPobj.TaxDate = DateTime.Now;
                SAPobj.Series = obj.TargetSeries;

                if (!string.IsNullOrEmpty(obj.Comments))
                    SAPobj.Comments = obj.Comments;

                Documents order = (Documents)_company.GetBusinessObject(BoObjectTypes.oOrders);

                if (!order.GetByKey((int)obj.DocEntry))
                    return response.Error(404, $"Orden [{obj.DocEntry}] no encontrada");

                SAPobj.CardCode = order.CardCode;
                SAPobj.CardName = order.CardName;
                SAPobj.SalesPersonCode = order.SalesPersonCode;

                // copiar campos de usuario - header
                for (int i = 0; i < order.UserFields.Fields.Count; i++)
                {
                    try
                    {
                        string name = order.UserFields.Fields.Item(i).Name;
                        var value = order.UserFields.Fields.Item(i).Value;
                        SAPobj.UserFields.Fields.Item(name).Value = value;
                    }
                    catch { }
                }

                
                for (int i = 0; i < order.Lines.Count; i++)
                {
                    order.Lines.SetCurrentLine(i);

                    if (order.Lines.LineStatus == BoStatus.bost_Open)
                    {
                        SAPobj.Lines.BaseEntry = (int)obj.DocEntry;
                        SAPobj.Lines.BaseType = (int)BoObjectTypes.oOrders;
                        SAPobj.Lines.BaseLine = order.Lines.LineNum;
                        SAPobj.Lines.Quantity = order.Lines.RemainingOpenQuantity;
                        SAPobj.Lines.SerialNum = order.Lines.SerialNum;

                        // campos de usuario - linea
                        for (int u = 0; u < order.Lines.UserFields.Fields.Count; u++)
                        {
                            try
                            {
                                string name = order.Lines.UserFields.Fields.Item(u).Name;
                                var value = order.Lines.UserFields.Fields.Item(u).Value;
                                SAPobj.Lines.UserFields.Fields.Item(name).Value = value;
                            }
                            catch { }
                        }

                        // copiar series
                        for (int s = 0; s < order.Lines.SerialNumbers.Count; s++)
                        {
                            order.Lines.SerialNumbers.SetCurrentLine(s);
                            SAPobj.Lines.SerialNumbers.SystemSerialNumber = order.Lines.SerialNumbers.SystemSerialNumber;
                            SAPobj.Lines.SerialNumbers.Add();
                        }

                        SAPobj.Lines.Add();
                    }
                }

                int ret = SAPobj.Add();

                if (ret != 0)
                {
                    _company.GetLastError(out int errCode, out string errMsg);
                    return response.Error(500, $"Error SAP ODLN: {errCode} - {errMsg}");
                }

                int docEntry = int.Parse(_company.GetNewObjectKey());

                return response.Ok(new SAPObjResult
                {
                    DocEntry = docEntry,
                    DocType = BoObjectTypes.oDeliveryNotes
                });
            }
            catch (Exception ex)
            {
                return response.Error(500, $"Error SAP ODLN: {ex.Message}");
            }
            finally
            {
                if (SAPobj != null)
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(SAPobj);
            }
        }

        public ApiResponse CopyOrderToInvoice(ORDR obj)
        {
            ApiResponse response = new ApiResponse();
            Documents SAPobj = null;
            Documents order = null;
            
            try
            {
                order = (Documents)_company.GetBusinessObject(BoObjectTypes.oOrders);

                if (!order.GetByKey((int)obj.DocEntry))
                    return response.Error(404, $"Orden [{obj.DocEntry}] no encontrada");

                SAPobj = (Documents)_company.GetBusinessObject(BoObjectTypes.oInvoices);

                SAPobj.CardCode = order.CardCode;
                SAPobj.CardName = order.CardName;
                SAPobj.DocDate = DateTime.Now;
                SAPobj.TaxDate = DateTime.Now;
                SAPobj.Series = obj.TargetSeries;
                SAPobj.SalesPersonCode = order.SalesPersonCode;

                if (!string.IsNullOrEmpty(obj.Comments))
                    SAPobj.Comments = obj.Comments;

                // copiar campos de usuario - header
                for (int i = 0; i < order.UserFields.Fields.Count; i++)
                {
                    try
                    {
                        string name = order.UserFields.Fields.Item(i).Name;
                        var value = order.UserFields.Fields.Item(i).Value;
                        SAPobj.UserFields.Fields.Item(name).Value = value;
                    }
                    catch { }
                }

                // copiar lineas
                for (int i = 0; i < order.Lines.Count; i++)
                {
                    order.Lines.SetCurrentLine(i);

                    if (order.Lines.LineStatus == BoStatus.bost_Open)
                    {
                        SAPobj.Lines.BaseEntry = (int)obj.DocEntry;
                        SAPobj.Lines.BaseType = (int)BoObjectTypes.oOrders;
                        SAPobj.Lines.BaseLine = order.Lines.LineNum;
                        SAPobj.Lines.SerialNum = order.Lines.SerialNum;

                        SAPobj.Lines.Quantity = order.Lines.RemainingOpenQuantity;

                        // campos de usuario - linea
                        for (int u = 0; u < order.Lines.UserFields.Fields.Count; u++)
                        {
                            try
                            {
                                string name = order.Lines.UserFields.Fields.Item(u).Name;
                                var value = order.Lines.UserFields.Fields.Item(u).Value;
                                SAPobj.Lines.UserFields.Fields.Item(name).Value = value;
                            }
                            catch { }
                        }

                        // copiar series
                        for (int s = 0; s < order.Lines.SerialNumbers.Count; s++)
                        {
                            order.Lines.SerialNumbers.SetCurrentLine(s);
                            SAPobj.Lines.SerialNumbers.SystemSerialNumber = order.Lines.SerialNumbers.SystemSerialNumber;
                            SAPobj.Lines.SerialNumbers.Add();
                        }

                        SAPobj.Lines.Add();
                    }
                }

                int ret = SAPobj.Add();

                if (ret != 0)
                {
                    _company.GetLastError(out int errCode, out string errMsg);
                    return response.Error(500, $"Error SAP OINV: {errCode} - {errMsg}");
                }

                int docEntry = int.Parse(_company.GetNewObjectKey());

                return response.Ok(new SAPObjResult
                {
                    DocEntry = docEntry,
                    DocType = BoObjectTypes.oInvoices
                });
            }
            catch (Exception ex)
            {
                return response.Error(500, $"Error SAP OINV: {ex.Message}");
            }
            finally
            {
                if (SAPobj != null) System.Runtime.InteropServices.Marshal.ReleaseComObject(SAPobj);
                if (order != null) System.Runtime.InteropServices.Marshal.ReleaseComObject(order);
            }
        }

        // -- entrega
        public ApiResponse CreateDelivery(ODLN obj)
        {
            ApiResponse response = new ApiResponse();
            Documents SAPobj = null;

            try
            {
                SAPobj = (Documents)_company.GetBusinessObject(BoObjectTypes.oDeliveryNotes);

                SAPobj.CardCode = obj.CardCode;
                if (!string.IsNullOrEmpty(obj.CardName)) SAPobj.CardName = obj.CardName;
                SAPobj.DocDate = DateTime.Now;
                SAPobj.TaxDate = DateTime.Now;
                SAPobj.Series = obj.Series;
                SAPobj.Comments = obj.Comments;
                SAPobj.SalesPersonCode = obj.SalesPersonCode;

                foreach (var UF in obj.UserFields ?? [])
                {
                    try
                    {
                        SAPobj.UserFields.Fields.Item(UF.Key).Value = UF.Value;
                    }
                    catch { }
                }

                int LineNum = 0;
                foreach (var item in obj.Lines)
                {
                    SAPobj.Lines.SetCurrentLine(LineNum);

                    if (item.BaseEntry != null)
                    {
                        SAPobj.Lines.BaseEntry = (int)item.BaseEntry;
                        SAPobj.Lines.BaseLine = (int)item.BaseLine;
                        SAPobj.Lines.BaseType = (int)BoObjectTypes.oOrders;
                        SAPobj.Lines.Quantity = item.Quantity;
                    }
                    else
                    {
                        SAPobj.Lines.ItemCode = item.ItemCode;
                        SAPobj.Lines.Quantity = item.Quantity;
                        if(item.Price != null) SAPobj.Lines.Price = (double)item.Price;
                    }

                    if (!string.IsNullOrEmpty(item.SerialNumber))
                    {
                        SAPobj.Lines.UserFields.Fields.Item("U_MSERIE").Value = item.SerialNumber;

                        if (item.SysSerial != null)
                        {
                            SAPobj.Lines.SerialNumbers.SystemSerialNumber = (int)item.SysSerial;
                            SAPobj.Lines.SerialNumbers.Add();
                        }
                    }

                    // campos de usuario por linea
                    foreach (var UF in item.UserFields ?? [])
                    {
                        try
                        {
                            SAPobj.Lines.UserFields.Fields.Item(UF.Key).Value = UF.Value;
                        }
                        catch { }
                    }

                    SAPobj.Lines.Add();
                    LineNum++;
                }

                int ret = SAPobj.Add();

                if (ret != 0)
                {
                    _company.GetLastError(out int errCode, out string errMsg);
                    return response.Error(500, $"Error SAP ODLN: {errCode} - {errMsg}");
                }

                int docEntry = int.Parse(_company.GetNewObjectKey());

                return response.Ok(new SAPObjResult
                {
                    DocEntry = docEntry,
                    DocType = BoObjectTypes.oDeliveryNotes
                });
            }
            catch (Exception ex)
            {
                return response.Error(500, $"Error SAP ODLN: {ex.Message}");
            }
            finally
            {
                if (SAPobj != null)
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(SAPobj);
            }
        }

        public ApiResponse CopyDeliveryToInvoice(ODLN obj)
        {
            ApiResponse response = new ApiResponse();
            Documents SAPobj = null;
            Documents BaseDoc = null;

            try
            {
                BaseDoc = (Documents)_company.GetBusinessObject(BoObjectTypes.oDeliveryNotes);

                if (!BaseDoc.GetByKey((int)obj.DocEntry))
                    return response.Error(404, $"Entrega [{obj.DocEntry}] no encontrada");

                SAPobj = (Documents)_company.GetBusinessObject(BoObjectTypes.oInvoices);

                SAPobj.CardCode = BaseDoc.CardCode;
                SAPobj.CardName = BaseDoc.CardName;
                SAPobj.DocDate = DateTime.Now;
                SAPobj.TaxDate = DateTime.Now;
                SAPobj.Series = obj.TargetSeries;
                SAPobj.SalesPersonCode = BaseDoc.SalesPersonCode;

                if (!string.IsNullOrEmpty(obj.Comments))
                    SAPobj.Comments = obj.Comments;

                // copiar campos de usuario - header
                for (int i = 0; i < BaseDoc.UserFields.Fields.Count; i++)
                {
                    try
                    {
                        string name = BaseDoc.UserFields.Fields.Item(i).Name;
                        var value = BaseDoc.UserFields.Fields.Item(i).Value;
                        SAPobj.UserFields.Fields.Item(name).Value = value;
                    }
                    catch { }
                }

                for (int i = 0; i < BaseDoc.Lines.Count; i++)
                {
                    BaseDoc.Lines.SetCurrentLine(i);

                    if (BaseDoc.Lines.LineStatus == BoStatus.bost_Open)
                    {
                        SAPobj.Lines.BaseEntry = (int)obj.DocEntry;
                        SAPobj.Lines.BaseType = (int)BoObjectTypes.oDeliveryNotes;
                        SAPobj.Lines.BaseLine = BaseDoc.Lines.LineNum;
                        SAPobj.Lines.SerialNum = BaseDoc.Lines.SerialNum;
                        SAPobj.Lines.Quantity = BaseDoc.Lines.RemainingOpenQuantity;

                        // campos de usuario - linea
                        for (int u = 0; u < BaseDoc.Lines.UserFields.Fields.Count; u++)
                        {
                            try
                            {
                                string name = BaseDoc.Lines.UserFields.Fields.Item(u).Name;
                                var value = BaseDoc.Lines.UserFields.Fields.Item(u).Value;
                                SAPobj.Lines.UserFields.Fields.Item(name).Value = value;
                            }
                            catch { }
                        }

                        // copiar series
                        for (int s = 0; s < BaseDoc.Lines.SerialNumbers.Count; s++)
                        {
                            BaseDoc.Lines.SerialNumbers.SetCurrentLine(s);
                            SAPobj.Lines.SerialNumbers.SystemSerialNumber = BaseDoc.Lines.SerialNumbers.SystemSerialNumber;
                            SAPobj.Lines.SerialNumbers.Add();
                        }

                        SAPobj.Lines.Add();
                    }
                }

                int ret = SAPobj.Add();

                if (ret != 0)
                {
                    _company.GetLastError(out int errCode, out string errMsg);
                    return response.Error(500, $"Error SAP OINV: {errCode} - {errMsg}");
                }

                int docEntry = int.Parse(_company.GetNewObjectKey());

                return response.Ok(new SAPObjResult
                {
                    DocEntry = docEntry,
                    DocType = BoObjectTypes.oInvoices
                });
            }
            catch (Exception ex)
            {
                return response.Error(500, $"Error SAP OINV: {ex.Message}");
            }
            finally
            {
                if (SAPobj != null) System.Runtime.InteropServices.Marshal.ReleaseComObject(SAPobj);
                if (BaseDoc != null) System.Runtime.InteropServices.Marshal.ReleaseComObject(BaseDoc);
            }
        }

        public ApiResponse CopyDeliveryToReturn(ODLN obj)
        {
            ApiResponse response = new ApiResponse();
            Documents SAPobj = null;
            Documents BaseDoc = null;

            try
            {
                BaseDoc = (Documents)_company.GetBusinessObject(BoObjectTypes.oDeliveryNotes);

                if (!BaseDoc.GetByKey((int)obj.DocEntry))
                    return response.Error(404, $"Entrega [{obj.DocEntry}] no encontrada");

                SAPobj = (Documents)_company.GetBusinessObject(BoObjectTypes.oReturns);

                SAPobj.CardCode = BaseDoc.CardCode;
                SAPobj.CardName = BaseDoc.CardName;
                SAPobj.DocDate = DateTime.Now;
                SAPobj.TaxDate = DateTime.Now;
                SAPobj.Series = obj.TargetSeries;
                SAPobj.SalesPersonCode = BaseDoc.SalesPersonCode;

                if (!string.IsNullOrEmpty(obj.Comments))
                    SAPobj.Comments = obj.Comments;

                // ----------- copiar campos de usuario - header
                for (int i = 0; i < BaseDoc.UserFields.Fields.Count; i++)
                {
                    try
                    {
                        string name = BaseDoc.UserFields.Fields.Item(i).Name;
                        var value = BaseDoc.UserFields.Fields.Item(i).Value;
                        SAPobj.UserFields.Fields.Item(name).Value = value;
                    }
                    catch { }
                }

                foreach (var item in obj.UserFields)
                {
                    try
                    {
                        SAPobj.UserFields.Fields.Item(item.Key).Value = item.Value;
                    }
                    catch { }
                }

                // -----------

                for (int i = 0; i < BaseDoc.Lines.Count; i++)
                {
                    BaseDoc.Lines.SetCurrentLine(i);

                    if (BaseDoc.Lines.LineStatus == BoStatus.bost_Open)
                    {
                        SAPobj.Lines.BaseEntry = (int)obj.DocEntry;
                        SAPobj.Lines.BaseType = (int)BoObjectTypes.oDeliveryNotes;
                        SAPobj.Lines.BaseLine = BaseDoc.Lines.LineNum;
                        SAPobj.Lines.SerialNum = BaseDoc.Lines.SerialNum;
                        SAPobj.Lines.Quantity = BaseDoc.Lines.RemainingOpenQuantity;

                        // campos de usuario - linea
                        for (int u = 0; u < BaseDoc.Lines.UserFields.Fields.Count; u++)
                        {
                            try
                            {
                                string name = BaseDoc.Lines.UserFields.Fields.Item(u).Name;
                                var value = BaseDoc.Lines.UserFields.Fields.Item(u).Value;
                                SAPobj.Lines.UserFields.Fields.Item(name).Value = value;
                            }
                            catch { }
                        }

                        // copiar series
                        for (int s = 0; s < BaseDoc.Lines.SerialNumbers.Count; s++)
                        {
                            BaseDoc.Lines.SerialNumbers.SetCurrentLine(s);
                            SAPobj.Lines.SerialNumbers.SystemSerialNumber = BaseDoc.Lines.SerialNumbers.SystemSerialNumber;
                            SAPobj.Lines.SerialNumbers.Add();
                        }

                        SAPobj.Lines.Add();
                    }
                }

                int ret = SAPobj.Add();

                if (ret != 0)
                {
                    _company.GetLastError(out int errCode, out string errMsg);
                    return response.Error(500, $"Error SAP ORDN: {errCode} - {errMsg}");
                }

                int docEntry = int.Parse(_company.GetNewObjectKey());

                return response.Ok(new SAPObjResult
                {
                    DocEntry = docEntry,
                    DocType = BoObjectTypes.oReturns
                });
            }
            catch (Exception ex)
            {
                return response.Error(500, $"Error SAP ORDN: {ex.Message}");
            }
            finally
            {
                if (SAPobj != null) System.Runtime.InteropServices.Marshal.ReleaseComObject(SAPobj);
                if (BaseDoc != null) System.Runtime.InteropServices.Marshal.ReleaseComObject(BaseDoc);
            }
        }


        public ApiResponse CloseDelivery(int DocEntry)
        {
            ApiResponse response = new ApiResponse();
            Documents BaseDoc = null;

            try
            {
                BaseDoc = (Documents)_company.GetBusinessObject(BoObjectTypes.oDeliveryNotes);

                if (!BaseDoc.GetByKey(DocEntry))
                    return response.Error(404, $"Entrega [{DocEntry}] no encontrada");

                for (int i = 0; i < BaseDoc.Lines.Count; i++)
                {
                    BaseDoc.Lines.SetCurrentLine(i);

                    if (BaseDoc.Lines.LineStatus == BoStatus.bost_Open)
                    {
                        BaseDoc.Lines.LineStatus = BoStatus.bost_Close;
                    }
                }

                int ret = BaseDoc.Update();

                if (ret != 0)
                {
                    _company.GetLastError(out int errCode, out string errMsg);
                    return response.Error(500, $"Error SAP ODLN: {errCode} - {errMsg}");
                }

                return response.Ok(new SAPObjResult
                {
                    DocEntry = DocEntry,
                    DocType = BoObjectTypes.oDeliveryNotes
                });
            }
            catch (Exception ex)
            {
                return response.Error(500, $"Error SAP ODLN: {ex.Message}");
            }
            finally
            {
                if (BaseDoc != null)
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(BaseDoc);
            }
        }

        // factura de deudores
        public ApiResponse CreateInvoice(OINV obj)
        {
            ApiResponse response = new ApiResponse();
            Documents SAPobj = null;

            try
            {
                SAPobj = (Documents)_company.GetBusinessObject(BoObjectTypes.oInvoices);

                SAPobj.CardCode = obj.CardCode;
                if (!string.IsNullOrEmpty(obj.CardName)) SAPobj.CardName = obj.CardName;

                SAPobj.DocDate = DateTime.Now;
                SAPobj.TaxDate = DateTime.Now;
                SAPobj.Series = obj.Series;
                SAPobj.Comments = obj.Comments;
                SAPobj.SalesPersonCode = obj.SalesPersonCode;

                if (obj.UserFields != null)
                {
                    foreach (var UF in obj.UserFields)
                    {
                        try
                        {
                            SAPobj.UserFields.Fields.Item(UF.Key).Value = UF.Value;
                        }
                        catch { }
                    }
                }

                int LineNum = 0;
                foreach (var item in obj.Lines)
                {
                    SAPobj.Lines.SetCurrentLine(LineNum);

                    if (item.BaseEntry != null)
                    {
                        SAPobj.Lines.BaseEntry = (int)item.BaseEntry;
                        SAPobj.Lines.BaseLine = (int)item.BaseLine;
                        SAPobj.Lines.BaseType = (int)BoObjectTypes.oDeliveryNotes;
                        SAPobj.Lines.Quantity = item.Quantity;
                    }
                    else
                    {
                        SAPobj.Lines.ItemCode = item.ItemCode;
                        SAPobj.Lines.Quantity = item.Quantity;
                        SAPobj.Lines.Price = item.Price;
                    }

                    if (!string.IsNullOrEmpty(item.SerialNumber))
                    {
                        SAPobj.Lines.UserFields.Fields.Item("U_MSERIE").Value = item.SerialNumber;

                        if (item.SysSerial != null)
                        {
                            SAPobj.Lines.SerialNumbers.SystemSerialNumber = (int)item.SysSerial;
                            SAPobj.Lines.SerialNumbers.Add();
                        }
                    }

                    SAPobj.Lines.Add();
                    LineNum++;
                }

                int ret = SAPobj.Add();

                if (ret != 0)
                {
                    _company.GetLastError(out int errCode, out string errMsg);
                    return response.Error(500, $"Error SAP OINV: {errCode} - {errMsg}");
                }

                int docEntry = int.Parse(_company.GetNewObjectKey());

                return response.Ok(new SAPObjResult
                {
                    DocEntry = docEntry,
                    DocType = BoObjectTypes.oInvoices
                });
            }
            catch (Exception ex)
            {
                return response.Error(500, $"Error SAP OINV: {ex.Message}");
            }
            finally
            {
                if (SAPobj != null)
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(SAPobj);
            }
        }

        /// <summary>
        /// Actualiza el encabezado de una OV.
        /// - DiscPrcnt: aplica el porcentaje de descuento directamente.
        /// - DocTotal: calcula el DiscPrcnt necesario para alcanzar ese total deseado
        ///   (útil cuando se conoce el monto final pero no el porcentaje).
        /// - Comments: actualiza el comentario del documento.
        /// </summary>
        public ApiResponse UpdateSalesOrderHeader(ORDR obj)
        {
            ApiResponse response = new ApiResponse();
            Documents SAPobj = null;
            try
            {
                SAPobj = (Documents)_company.GetBusinessObject(BoObjectTypes.oOrders);

                if (!SAPobj.GetByKey((int)obj.DocEntry))
                    return response.Error(404, $"Orden [{obj.DocEntry}] no encontrada.");

                if (obj.DiscPrcnt != null)
                {
                    SAPobj.DiscountPercent = (double)obj.DiscPrcnt;
                }
                else if (obj.DocTotal != null)
                {
                    //SAPobj.DocTotal = (double)obj.DocTotal;
                    double currentTotal = SAPobj.DocTotal;
                    double desiredTotal = (double)obj.DocTotal;
                    if (currentTotal > 0 && desiredTotal < currentTotal)
                        SAPobj.DiscountPercent = (currentTotal - desiredTotal) / currentTotal * 100.0;
                }

                if (!string.IsNullOrEmpty(obj.Comments))
                    SAPobj.Comments = obj.Comments;

                int ret = SAPobj.Update();
                if (ret != 0)
                {
                    _company.GetLastError(out int errCode, out string errMsg);
                    return response.Error(500, $"Error SAP ORDR Update/Header: {errCode} - {errMsg}");
                }

                return response.Ok(obj.DocEntry);
            }
            catch (Exception ex)
            {
                return response.Error(500, ex.Message);
            }
            finally
            {
                if (SAPobj != null)
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(SAPobj);
            }
        }

        /// <summary>
        /// Actualiza una o más líneas existentes en una OV (Price y/o DiscountPercent por LineNum).
        /// </summary>
        public ApiResponse UpdateSalesOrderLine(ORDR obj)
        {
            ApiResponse response = new ApiResponse();
            Documents SAPobj = null;
            try
            {
                SAPobj = (Documents)_company.GetBusinessObject(BoObjectTypes.oOrders);

                if (!SAPobj.GetByKey((int)obj.DocEntry))
                    return response.Error(404, $"Orden [{obj.DocEntry}] no encontrada.");

                if (!string.IsNullOrEmpty(obj.Comments))
                    SAPobj.Comments = obj.Comments;

                foreach (var line in obj.Lines)
                {
                    bool found = false;
                    for (int i = 0; i < SAPobj.Lines.Count; i++)
                    {
                        SAPobj.Lines.SetCurrentLine(i);
                        if (SAPobj.Lines.LineNum == line.LineNum)
                        {
                            if (line.Price != null) SAPobj.Lines.Price = (double)line.Price;
                            if (line.DiscountPercent != null) SAPobj.Lines.DiscountPercent = (double)line.DiscountPercent;
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                        return response.Error(404, $"Línea [{line.LineNum}] no encontrada en orden [{obj.DocEntry}].");
                }

                int ret = SAPobj.Update();
                if (ret != 0)
                {
                    _company.GetLastError(out int errCode, out string errMsg);
                    return response.Error(500, $"Error SAP ORDR Update/Line: {errCode} - {errMsg}");
                }

                return response.Ok(obj.DocEntry);
            }
            catch (Exception ex)
            {
                return response.Error(500, ex.Message);
            }
            finally
            {
                if (SAPobj != null)
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(SAPobj);
            }
        }

        /// <summary>
        /// Agrega una o más líneas nuevas a una OV existente.
        /// El precio que llega debe ser SIN ISV — el TaxCode en la línea aplica el impuesto.
        /// </summary>
        public ApiResponse CreateSalesOrderLine(ORDR obj)
        {
            ApiResponse response = new ApiResponse();
            Documents SAPobj = null;
            try
            {
                SAPobj = (Documents)_company.GetBusinessObject(BoObjectTypes.oOrders);

                if (!SAPobj.GetByKey((int)obj.DocEntry))
                    return response.Error(404, $"Orden [{obj.DocEntry}] no encontrada.");

                if (!string.IsNullOrEmpty(obj.Comments))
                    SAPobj.Comments = obj.Comments;

                foreach (var line in obj.Lines)
                {
                    SAPobj.Lines.SetCurrentLine(SAPobj.Lines.Count - 1);
                    SAPobj.Lines.Add();

                    SAPobj.Lines.ItemCode = line.ItemCode;
                    SAPobj.Lines.Quantity = line.Quantity > 0 ? line.Quantity : 1;

                    if (!string.IsNullOrEmpty(line.WhsCode))
                        SAPobj.Lines.WarehouseCode = line.WhsCode;

                    if (line.Price != null)
                        SAPobj.Lines.UnitPrice = (double)line.Price;

                    if (!string.IsNullOrEmpty(line.TaxCode))
                        SAPobj.Lines.TaxCode = line.TaxCode;

                    if (line.DiscountPercent != null) SAPobj.Lines.DiscountPercent = (double)line.DiscountPercent;

                    foreach (var UF in line.UserFields ?? [])
                    {
                        try { SAPobj.Lines.UserFields.Fields.Item(UF.Key).Value = UF.Value; } catch { }
                    }
                }

                int ret = SAPobj.Update();
                if (ret != 0)
                {
                    _company.GetLastError(out int errCode, out string errMsg);
                    return response.Error(500, $"Error SAP ORDR Create/Line: {errCode} - {errMsg}");
                }

                return response.Ok(obj.DocEntry);
            }
            catch (Exception ex)
            {
                return response.Error(500, ex.Message);
            }
            finally
            {
                if (SAPobj != null)
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(SAPobj);
            }
        }

        public byte[] GenerateInvoicePdf(int docEntry)
        {
            CompanyService companyService = _company.GetCompanyService();

            ReportLayoutsService layoutService =
                (ReportLayoutsService)companyService.GetBusinessService(ServiceTypes.ReportLayoutsService);

            ReportLayoutParams layoutParams =
                (ReportLayoutParams)layoutService.GetDataInterface(
                    ReportLayoutsServiceDataInterfaces.rlsdiReportLayoutParams);

            layoutParams.LayoutCode = "INV20099";

            ReportLayout layout = layoutService.GetReportLayout(layoutParams);

            ReportLayoutPrintParams printParams =
                (ReportLayoutPrintParams)layoutService.GetDataInterface(
                    ReportLayoutsServiceDataInterfaces.rlsdiReportLayoutPrintParams);

            printParams.DocEntry = docEntry;
            printParams.LayoutCode = layout.LayoutCode;

            layoutService.Print(printParams);

            string pdfPath = @"C:\Temp\invoice.pdf";

            return File.ReadAllBytes(pdfPath);
        }

    }
}
