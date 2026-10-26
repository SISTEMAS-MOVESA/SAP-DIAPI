using IntegracionesSAP.Models;
using SAPbobsCOM;
using System.Runtime.InteropServices;

namespace IntegracionesSAP.Libs
{
    public class SAPConnection
    {
        private static Company _company;

        // Turno unico para usar DI API: _company es compartido por todas las peticiones, asi que
        // solo una operacion a la vez puede conectar, crear documentos y desconectar.
        private static readonly SemaphoreSlim _turno = new SemaphoreSlim(1, 1);
        private static readonly AsyncLocal<int> _turnosTomados = new AsyncLocal<int>();
        private static readonly TimeSpan _esperaMaximaTurno = TimeSpan.FromSeconds(90);

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
            return SAPConnection.GetMovesaTestCOM();
        }
        
        /// <summary>
        /// Compania conectada por la peticion que tiene el turno.
        /// </summary>
        public static Company Actual => _company;

        /// <summary>
        /// Espera el turno de DI API (maximo 90 s). Es reentrante: si la misma peticion ya lo tiene, no vuelve a esperar.
        /// </summary>
        public static void TomarTurno()
        {
            if (_turnosTomados.Value > 0)
            {
                _turnosTomados.Value++;
                return;
            }

            if (!_turno.Wait(_esperaMaximaTurno))
                throw new Exception($"SAP-DIAPI ocupado: otra operacion lleva mas de {_esperaMaximaTurno.TotalSeconds} segundos usando SAP. Intente de nuevo.");

            _turnosTomados.Value = 1;
        }

        /// <summary>
        /// Suelta el turno solo si esta peticion lo tiene, para que un Disconnect sin Connect previo no libere el turno de otra.
        /// </summary>
        public static void SoltarTurno()
        {
            if (_turnosTomados.Value == 0)
                return;

            _turnosTomados.Value--;
            if (_turnosTomados.Value == 0)
                _turno.Release();
        }

        public static void Connect()
        {
            Connect(GetDefaultCOM);
        }

        /// <summary>
        /// Toma el turno y conecta a la compania indicada. La compania se elige dentro del turno
        /// para que otra peticion no la cambie mientras esta conectando.
        /// </summary>
        public static void Connect(Func<Company> seleccionarCompania)
        {
            TomarTurno();

            try
            {
                seleccionarCompania();

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
                SoltarTurno();
                throw new Exception($"Error SAPConnection.Connect(): {ex.Message}");
            }
        }
        public static void Disconnect()
        {
            try
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
            finally
            {
                SoltarTurno();
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
