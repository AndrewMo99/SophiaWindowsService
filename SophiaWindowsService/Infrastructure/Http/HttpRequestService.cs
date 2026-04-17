using Newtonsoft.Json;
using SophiaWindowsService.Application.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SophiaWindowsService.Application.Abstractions;

namespace SophiaWindowsService.Infrastructure.Http
{
    public class HttpRequestService : IHttpRequestService
    {
        private readonly HttpClient _client;

        public HttpRequestService(HttpClient client)
        {
            _client = client;
        }

        public async Task<TResponse> SendAsync<TRequest, TResponse>(
            HttpMethod method,
            string endpoint,
            TRequest body = null,
            IDictionary<string, string> headers = null,
            CancellationToken cancellationToken = default)
            where TRequest : class
            where TResponse : class
        {
            try
            {
                using (HttpRequestMessage request = BuildRequest(method, endpoint, body, headers))
                {
                    LogExtensions.WriteEventLog($"Calling {method} {endpoint}", EventLogEntryType.Information);

                    var response = await _client.SendAsync(request, cancellationToken);

                    var jsonResponse = await response.Content.ReadAsStringAsync();

                    if (!response.IsSuccessStatusCode)
                    {
                        var errorMsg = $"HTTP Error in {endpoint}: {(int)response.StatusCode}. Detalle: {jsonResponse}";
                        LogExtensions.WriteEventLog(errorMsg, EventLogEntryType.Error);

                        throw new HttpRequestException(jsonResponse);
                    }

                    TResponse resultResponse = JsonConvert.DeserializeObject<TResponse>(jsonResponse);

                    if (resultResponse != null)
                    {
                        return resultResponse;
                    }

                    LogExtensions.WriteEventLog($"Response body was null from {endpoint}", EventLogEntryType.Warning);
                    throw new InvalidOperationException($"No content was returned by the request: {endpoint}");
                }
            }
            catch (Exception ex)
            {
                ex.GetErrorMessage().WriteLog();
                throw new InvalidOperationException($"An error occurred while processing the request: {endpoint}", ex);
            }
        }

        private static HttpRequestMessage BuildRequest<TRequest>(
            HttpMethod method,
            string endpoint,
            TRequest body,
            IDictionary<string, string> headers)
            where TRequest : class
        {
            var request = new HttpRequestMessage(method, endpoint);

            if (body != null)
            {
                string jsonBody = JsonConvert.SerializeObject(body);
                request.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
            }

            if (headers != null)
            {
                foreach (var header in headers)
                {
                    request.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            }

            return request;
        }
    }
}