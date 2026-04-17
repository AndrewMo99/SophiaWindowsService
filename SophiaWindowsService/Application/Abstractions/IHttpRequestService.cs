using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace SophiaWindowsService.Application.Abstractions
{
    public interface IHttpRequestService
    {
        Task<TResponse> SendAsync<TRequest, TResponse>(
            HttpMethod method,
            string endpoint,
            TRequest body = null,
            IDictionary<string, string> headers = null,
            CancellationToken cancellationToken = default)
            where TRequest : class
            where TResponse : class;
    }
}