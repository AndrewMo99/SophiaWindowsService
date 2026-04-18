namespace SophiaWindowsService.Application.Responses
{
    public class TokenResponse
    {
        public string StatusCode { get; set; }
        public TokenDataResponse Data { get; set; }
    }

    public class TokenDataResponse
    {
        public string Access_token { get; set; }
    }
}