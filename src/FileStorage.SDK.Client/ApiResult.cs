using Newtonsoft.Json;

namespace FileStorage.SDK.Client
{
    public class ApiResult
    {
        [JsonProperty("success")]
        public bool Success { get; set; }
        [JsonProperty("error")]
        public string Error { get; set; }
    }

    public class ApiResult<T> : ApiResult
    {
        [JsonProperty("data")]
        public T Data { get; set; }
    }
}
