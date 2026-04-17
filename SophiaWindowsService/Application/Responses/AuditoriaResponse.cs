namespace SophiaWindowsService.Application.Responses
{
    public class AuditoriaResponse
    {
        public string StatusCode { get; set; }
        public AuditoriaDataResponse Data { get; set; }
    }

    public class AuditoriaDataResponse
    {
        public string BundleId { get; set; }
    }
}