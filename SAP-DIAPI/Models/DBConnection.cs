namespace IntegracionesSAP.Models
{
    public class DBConnection
    {
        public string Server { get; set; }
        public string DataBase { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }

        override
        public string ToString()
        {
            return $"Server={Server};Database={DataBase};User Id={UserName};Password={Password};TrustServerCertificate=True;";
        }
    }
}
