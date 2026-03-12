namespace IntegracionesSAP.Models
{
    public class ApiResponse
    {
        public int Code { get; set; } = 200;
        public string Message { get; set; }
        public bool Success { get; set; }
        public object Data { get; set; }

        public ApiResponse Ok(object data = null, string message = "Operacion completada con exito")
        {
            Code = 200;
            Message = message;
            Success = true;
            Data = data;
            return this;
        }

        public ApiResponse Error(int code = 500, string message = "Error al realizar la operacion", object data = null)
        {
            Code = code;
            Message = message;
            Success = false;
            Data = data;
            return this;
        }

        public ApiResponse NotFound(string message = "Recurso no encontrado")
        {
            Code = 404;
            Message = message;
            Success = false;
            Data = null;
            return this;
        }

        public ApiResponse Forbidden(string message = "Accion no autorizada")
        {
            Code = 401;
            Message = message;
            Success = false;
            Data = null;
            return this;
        }
    }
}
