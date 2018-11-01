namespace FileStorage.Core
{
    public class ApiResult
    {
        public readonly bool Success;
        public readonly string Error;

        public ApiResult(bool success, string error)
        {
            Success = success;
            Error = error;
        }

        public ApiResult()
        {
            Success = true;
            Error = string.Empty;
        }

        public static ApiResult Failed(string error) => new ApiResult(false, error);

        public static ApiResult Succeed(string message = null) => new ApiResult(true, message);
    }

    public class ApiResult<T> : ApiResult
    {
        public readonly T Data;

        public ApiResult()
        {
            Data = default;
        }

        public ApiResult(T data, bool success = true, string error = null)
            :base(success, error)
        {
            Data = data;
        }
    }
}