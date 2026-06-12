using IntegracionesSAP.Models;
using SAPbobsCOM;
using System.Runtime.InteropServices;

namespace IntegracionesSAP.Libs
{
    public class SAPConnection
    {
        private static Company _company;
        private static string _user = "it";
        private static string _password = "polar";
        
        public static Company GetMovesaCOM()
        {
            DBConnection conn = MSSQL.DB_MOVESA;
            if (_company != null && _company.Connected)
                return _company;

            _company = new Company
            {
                Server = conn.Server,
                CompanyDB = conn.DataBase,
                DbServerType = BoDataServerTypes.dst_MSSQL2012,
                DbUserName = conn.UserName,
                DbPassword = conn.Password,
                UserName = _user,
                Password = _password,
                language = BoSuppLangs.ln_Spanish_La,
                LicenseServer = "192.168.1.9",
                SLDServer = "192.168.1.9:40000"
            };

            return _company;
        }

        public static Company GetMovesaTestCOM()
        {
            DBConnection conn = MSSQL.DB_MOVESA_TEST;
            if (_company != null && _company.Connected)
                return _company;

            _company = new Company
            {
                Server = conn.Server,
                CompanyDB = conn.DataBase,
                DbServerType = BoDataServerTypes.dst_MSSQL2012,
                DbUserName = conn.UserName,
                DbPassword = conn.Password,
                UserName = _user,
                Password = _password,
                language = BoSuppLangs.ln_Spanish_La,
                LicenseServer = "192.168.1.9",
                SLDServer = "192.168.1.9:40000"
            };

            return _company;
        }

        public static Company GetABCompanyCOM()
        {
            DBConnection conn = MSSQL.DB_ABCOMPANY;
            if (_company != null && _company.Connected)
                return _company;

            _company = new Company
            {
                Server = conn.Server,
                CompanyDB = conn.DataBase,
                DbServerType = BoDataServerTypes.dst_MSSQL2012,
                DbUserName = conn.UserName,
                DbPassword = conn.Password,
                UserName = _user,
                Password = _password,
                language = BoSuppLangs.ln_Spanish_La,
                LicenseServer = "192.168.1.9",
                SLDServer = "192.168.1.9:40000"
            };

            return _company;

        }

        public static Company GetVehicoCOM()
        {
            DBConnection conn = MSSQL.DB_VEHICO;
            if (_company != null && _company.Connected)
                return _company;

            _company = new Company
            {
                Server = conn.Server,
                CompanyDB = conn.DataBase,
                DbServerType = BoDataServerTypes.dst_MSSQL2012,
                DbUserName = conn.UserName,
                DbPassword = conn.Password,
                UserName = _user,
                Password = _password,
                language = BoSuppLangs.ln_Spanish_La,
                LicenseServer = "192.168.1.9",
                SLDServer = "192.168.1.9:40000"
            };

            return _company;

        }

        public static Company GetDefaultCOM()
        {
            return SAPConnection.GetMovesaCOM();
        }
        
        public static void Connect()
        {
            if (_company == null)
                GetDefaultCOM();

            try
            {
                if (!_company.Connected)
                {
                    int result = _company.Connect();

                    if (result != 0)
                    {
                        _company.GetLastError(out int errCode, out string errMsg);
                        throw new Exception($"Error SAPConnection.GetMovesaCOM(): {errCode} - {errMsg}");
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error SAPConnection.Connect(): {ex.Message}");
            }
        }
        public static void Disconnect()
        {
            if (_company != null)
            {
                try
                {
                    if (_company.Connected)
                    {
                        _company.Disconnect();
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception($"Error SAPConnection.Disconnect(): {ex.Message}");
                }
                finally
                {
                    if (_company != null)
                    {
                        Marshal.ReleaseComObject(_company);
                        _company = null;
                    }
                }
            }
        }
    
        public static int GetObjDocNum(string Table, int DocEntry)
        {
            int DocNum = MSSQL.ExecuteScalar<int>(
                MSSQL.DB_DEFAULT,
                $"SELECT ISNULL(MAX(DocNum),0) FROM [{Table}] WHERE DocEntry = @DocEntry",
                new Dictionary<string, object>
                {
                    { "@DocEntry", DocEntry }
                }
            );

            return DocNum;
        }
    }
}
