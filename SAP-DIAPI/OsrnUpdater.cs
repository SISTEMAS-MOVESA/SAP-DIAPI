using Microsoft.Data.SqlClient;

public class OsrnUpdater
{
    private readonly string _connectionString;

    public OsrnUpdater(string connectionString)
    {
        _connectionString = connectionString;
    }

    public bool ActualizarOSRN(
        string mnfSerial,
        string nAduana,
        DateTime fPago,
        string nPoliza,
        string nItem,
        string codRep)
    {
        bool resultado = false;

        string sql = @"
            UPDATE OSRN SET 
                U_NADUANA   = @NADUANA,
                U_FPAGO     = @FPAGO,
                U_NPOLIZA   = @NPOLIZA,
                U_NITEM     = @NITEM,
                U_CODIGOREP = @CODIGOREP,
                U_Estado_Moto = '01',
                U_UBICACION_DCM = '100000080'
            WHERE MNFSERIAL = @MNFSERIAL";

        try
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@NADUANA", nAduana);
                cmd.Parameters.AddWithValue("@FPAGO", fPago);
                cmd.Parameters.AddWithValue("@NPOLIZA", nPoliza);
                cmd.Parameters.AddWithValue("@NITEM", nItem);
                cmd.Parameters.AddWithValue("@CODIGOREP", codRep);
                cmd.Parameters.AddWithValue("@MNFSERIAL", mnfSerial);

                conn.Open();
                int filas = cmd.ExecuteNonQuery();
                resultado = filas > 0;
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Error SQL al actualizar OSRN: {ex.Message}");
        }

        return resultado;
    }
}
