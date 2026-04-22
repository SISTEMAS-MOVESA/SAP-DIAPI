using IntegracionesSAP.Models;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Text.RegularExpressions;

namespace IntegracionesSAP.Libs
{
    public class MSSQL
    {
        public static DBConnection DB_MOVESA = new DBConnection() 
        {
            Server = "192.168.1.3", DataBase = "MOVESA", UserName = "sa", Password = "M*l!n3r0s2k12"
        };

        public static DBConnection DB_MOVESA_TEST = new DBConnection()
        {
            Server = "192.168.1.3", DataBase = "MOVESA_TSET", UserName = "sa", Password = "M*l!n3r0s2k12"
        };

        public static DBConnection DB_ABCOMPANY = new DBConnection()
        {
            Server = "192.168.1.3", DataBase = "ABCOMPANY", UserName = "sa", Password = "M*l!n3r0s2k12"
        };

        public static DBConnection DB_DEFAULT = MSSQL.DB_MOVESA_TEST;

        public static T ExecuteScalar<T>(DBConnection db, string query, Dictionary<string, object>? parameters)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(db.ToString()))
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.CommandType = CommandType.Text;

                    if (parameters != null)
                    {
                        foreach (var p in parameters)
                            cmd.Parameters.AddWithValue(p.Key, p.Value ?? DBNull.Value);
                    }

                    conn.Open();

                    object result = cmd.ExecuteScalar();

                    if (result == null || result == DBNull.Value)
                        return default!;

                    return (T)Convert.ChangeType(result, typeof(T));
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error MSSQL.ExecuteScalar: {ex.Message}", ex);
            }
        }

        public static List<Dictionary<string, object>> ExecuteQuery(
            DBConnection db, string query, Dictionary<string, object>? parameters = null
        ){
            try
            {
                using (SqlConnection conn = new SqlConnection(db.ToString()))
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.CommandType = CommandType.Text;

                    if (parameters != null)
                    {
                        foreach (var p in parameters)
                            cmd.Parameters.AddWithValue(p.Key, p.Value ?? DBNull.Value);
                    }

                    conn.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        var rows = new List<Dictionary<string, object>>();

                        while (reader.Read())
                        {
                            var row = new Dictionary<string, object>();

                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                row[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
                            }

                            rows.Add(row);
                        }

                        return rows;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error MSSQL.ExecuteQuery: {ex.Message}", ex);
            }
        }

        public static int ExecuteNonQuery(
            DBConnection db, string query, Dictionary<string, object>? parameters = null
        ){
            try
            {
                using (SqlConnection conn = new SqlConnection(db.ToString()))
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.CommandType = CommandType.Text;

                    if (parameters != null)
                    {
                        foreach (var p in parameters)
                            cmd.Parameters.AddWithValue(p.Key, p.Value ?? DBNull.Value);
                    }

                    conn.Open();

                    return cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error MSSQL.ExecuteNonQuery: {ex.Message}", ex);
            }
        }

        public static object Trim(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            return Regex.Replace(value.Trim(), @"\s+", " ");
        }

        public static string Left(string text, int length)
        {
            if (string.IsNullOrEmpty(text)) return "";
            return text.Substring(0, Math.Min(text.Length, length));
        }
    }
}
