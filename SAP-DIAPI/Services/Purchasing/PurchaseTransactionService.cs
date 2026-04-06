using IntegracionesSAP.Libs;
using IntegracionesSAP.Models;
using IntegracionesSAP.Models.Purchasing;
using IntegracionesSAP.Models.Sales;
using SAPbobsCOM;

namespace IntegracionesSAP.Services.Purchasing
{
    public class PurchaseTransactionService
    {
        private Company _company;

        public PurchaseTransactionService()
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
        #region CREATE

        public ApiResponse CreatePurchaseRequest(OPRQ obj)
        {
            ApiResponse response = new ApiResponse();
            Documents doc = null;

            try
            {
                doc = (Documents)_company.GetBusinessObject(BoObjectTypes.oPurchaseRequest);

                doc.DocDate = DateTime.Now;
                doc.RequriedDate = obj.DocDueDate;
                doc.Comments = obj.Comments;

                // UDF Header
                CopyUserFields(obj.UserFields, doc.UserFields);

                int i = 0;
                foreach (var line in obj.Lines)
                {
                    doc.Lines.SetCurrentLine(i);

                    doc.Lines.ItemCode = line.ItemCode;
                    doc.Lines.Quantity = line.Quantity;
                    doc.Lines.RequiredDate = obj.DocDueDate;
                    doc.Lines.AccountCode = line.AccountCode;

                    CopyUserFields(line.UserFields, doc.Lines.UserFields);

                    doc.Lines.Add();
                    i++;
                }

                int ret = doc.Add();

                if (ret != 0)
                {
                    _company.GetLastError(out int errCode, out string errMsg);
                    return response.Error(500, $"Error SAP OPRQ: {errCode} - {errMsg}");
                }

                return response.Ok(int.Parse(_company.GetNewObjectKey()));
            }
            catch (Exception ex)
            {
                return response.Error(500, ex.Message);
            }
            finally
            {
                if (doc != null) System.Runtime.InteropServices.Marshal.ReleaseComObject(doc);
            }
        }

        public ApiResponse CreatePurchaseQuotation(OPQT obj)
        {
            ApiResponse response = new ApiResponse();
            Documents doc = null;

            try
            {
                doc = (Documents)_company.GetBusinessObject(BoObjectTypes.oPurchaseQuotations);

                doc.CardCode = obj.CardCode;
                doc.DocDate = DateTime.Now;
                doc.DocDueDate = obj.DocDueDate;
                doc.Comments = obj.Comments;

                CopyUserFields(obj.UserFields, doc.UserFields);

                int i = 0;
                foreach (var line in obj.Lines)
                {
                    doc.Lines.SetCurrentLine(i);

                    doc.Lines.ItemCode = line.ItemCode;
                    doc.Lines.Quantity = line.Quantity;
                    doc.Lines.Price = line.UnitPrice;
                    doc.Lines.AccountCode = line.AccountCode;

                    CopyUserFields(line.UserFields, doc.Lines.UserFields);

                    doc.Lines.Add();
                    i++;
                }

                int ret = doc.Add();

                if (ret != 0)
                {
                    _company.GetLastError(out int errCode, out string errMsg);
                    return response.Error(500, $"Error SAP OPQT: {errCode} - {errMsg}");
                }

                return response.Ok(int.Parse(_company.GetNewObjectKey()));
            }
            catch (Exception ex)
            {
                return response.Error(500, ex.Message);
            }
            finally
            {
                if (doc != null) System.Runtime.InteropServices.Marshal.ReleaseComObject(doc);
            }
        }

        public ApiResponse CreatePurchaseOrder(OPOR obj)
        {
            ApiResponse response = new ApiResponse();
            Documents doc = null;

            try
            {
                doc = (Documents)_company.GetBusinessObject(BoObjectTypes.oPurchaseOrders);

                doc.CardCode = obj.CardCode;
                doc.DocDate = DateTime.Now;
                doc.DocDueDate = obj.DocDueDate;
                doc.Comments = obj.Comments;
                doc.DocType = obj.DocType;

                CopyUserFields(obj.UserFields, doc.UserFields);

                foreach (var line in obj.Lines)
                {
                    doc.Lines.SetCurrentLine((int)line.LineNum);

                    if(obj.DocType == BoDocumentTypes.dDocument_Items)
                    {
                        doc.Lines.ItemCode = line.ItemCode;
                    }
                    else
                    {
                        doc.Lines.ItemDescription = line.ItemDescription;
                    }
                    
                    doc.Lines.Quantity = line.Quantity;
                    doc.Lines.Price = line.UnitPrice;
                    doc.Lines.AccountCode = line.AccountCode;
                    doc.Lines.TaxCode = line.TaxCode;

                    CopyUserFields(line.UserFields, doc.Lines.UserFields);

                    doc.Lines.Add();
                }

                int ret = doc.Add();

                if (ret != 0)
                {
                    _company.GetLastError(out int errCode, out string errMsg);
                    return response.Error(500, $"Error SAP OPOR: {errCode} - {errMsg}");
                }

                return response.Ok(int.Parse(_company.GetNewObjectKey()));
            }
            catch (Exception ex)
            {
                return response.Error(500, ex.Message);
            }
            finally
            {
                if (doc != null) System.Runtime.InteropServices.Marshal.ReleaseComObject(doc);
            }
        }

        #endregion

        #region COPY

        public ApiResponse CopyRequestToQuotation(int docEntry, int series)
        {
            return CopyDocument(
                docEntry,
                series,
                BoObjectTypes.oPurchaseRequest,
                BoObjectTypes.oPurchaseQuotations,
                "OPRQ -> OPQT"
            );
        }

        public ApiResponse CopyQuotationToOrder(int docEntry, int series)
        {
            return CopyDocument(
                docEntry,
                series,
                BoObjectTypes.oPurchaseQuotations,
                BoObjectTypes.oPurchaseOrders,
                "OPQT -> OPOR"
            );
        }

        private ApiResponse CopyDocument(int baseEntry, int targetSeries, BoObjectTypes baseType, BoObjectTypes targetType, string label)
        {
            ApiResponse response = new ApiResponse();
            Documents baseDoc = null;
            Documents newDoc = null;

            try
            {
                baseDoc = (Documents)_company.GetBusinessObject(baseType);

                if (!baseDoc.GetByKey(baseEntry))
                    return response.Error(404, $"Documento base [{baseEntry}] no encontrado");

                newDoc = (Documents)_company.GetBusinessObject(targetType);

                newDoc.CardCode = baseDoc.CardCode;
                newDoc.DocDate = DateTime.Now;
                newDoc.DocDueDate = baseDoc.DocDueDate;
                newDoc.Series = targetSeries;

                CopyUserFields((List<UserField>?)baseDoc.UserFields, newDoc.UserFields);

                for (int i = 0; i < baseDoc.Lines.Count; i++)
                {
                    baseDoc.Lines.SetCurrentLine(i);

                    if (baseDoc.Lines.LineStatus == BoStatus.bost_Open)
                    {
                        newDoc.Lines.BaseEntry = baseEntry;
                        newDoc.Lines.BaseType = (int)baseType;
                        newDoc.Lines.BaseLine = i;
                        newDoc.Lines.Quantity = baseDoc.Lines.RemainingOpenQuantity;

                        // copiar UDF líneas
                        CopyUserFieldsFromSource(baseDoc.Lines.UserFields, newDoc.Lines.UserFields);

                        newDoc.Lines.Add();
                    }
                }

                int ret = newDoc.Add();

                if (ret != 0)
                {
                    _company.GetLastError(out int errCode, out string errMsg);
                    return response.Error(500, $"Error SAP {label}: {errCode} - {errMsg}");
                }

                return response.Ok(int.Parse(_company.GetNewObjectKey()));
            }
            catch (Exception ex)
            {
                return response.Error(500, ex.Message);
            }
            finally
            {
                if (baseDoc != null) System.Runtime.InteropServices.Marshal.ReleaseComObject(baseDoc);
                if (newDoc != null) System.Runtime.InteropServices.Marshal.ReleaseComObject(newDoc);
            }
        }

        #endregion

        #region HELPERS

        private void CopyUserFields(List<UserField>? fields, UserFields target)
        {
            if (fields == null) return;

            foreach (var uf in fields)
            {
                try
                {
                    target.Fields.Item(uf.Key).Value = uf.Value;
                }
                catch { }
            }
        }

        private void CopyUserFieldsFromSource(UserFields source, UserFields target)
        {
            for (int i = 0; i < source.Fields.Count; i++)
            {
                try
                {
                    string name = source.Fields.Item(i).Name;
                    target.Fields.Item(name).Value = source.Fields.Item(i).Value;
                }
                catch { }
            }
        }

        #endregion
    }
}