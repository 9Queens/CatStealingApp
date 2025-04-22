using System.Net;

namespace CatApp.Shared.APIs.Local.v1.Responses
{



    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public HttpStatusCode Status { get; set; }
        public string? Message { get; set; }
        public T? Payload { get; set; }

        public static ApiResponse<T> Ok(T payload, string? message = null)
        {
            return new ApiResponse<T> { Success = true, Payload = payload, Message = message, Status = HttpStatusCode.OK };
        }

        //for when no erros yet not results either
        public static ApiResponse<T> NotFound(T payload, string? message = null)
        {
            return new ApiResponse<T> { Success = true, Payload = payload, Message = message, Status = HttpStatusCode.NotFound };
        }

        //strictly for erros (exceptions , internal server errors etc )
        public static ApiResponse<T> Fail(string message)
        {
            return new ApiResponse<T> { Success = false, Message = message, Status = HttpStatusCode.InternalServerError };
        }
    }

}
