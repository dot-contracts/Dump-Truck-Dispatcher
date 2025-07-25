using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Abp.UI;
using Castle.Core.Logging;
using DispatcherWeb.Fulcrum.Dto;
using Newtonsoft.Json;

namespace DispatcherWeb.Fulcrum
{
    public class FulcrumHttpClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger _logger;

        public FulcrumHttpClient(HttpClient httpClient, ILogger logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<List<T>> GetAllPages<T>(string requestPath, object payload = null, string accessToken = null)
        {
            var resultData = new List<T>();
            string nextLink = requestPath;

            do
            {
                var response = await SendFulcrumRequest(HttpMethod.Get, nextLink, payload, accessToken);
                var responseObject = JsonConvert.DeserializeObject<FulcrumPageResponse<T>>(response);

                if (responseObject.Data != null)
                {
                    resultData.AddRange(responseObject.Data);
                }

                nextLink = responseObject.Page.HasMore ? responseObject.Page.NextLink : null;

            } while (!string.IsNullOrEmpty(nextLink));

            return resultData;
        }

        public async Task<T> SendFulcrumRequest<T>(HttpMethod requestMethod, string requestPath, object payload = null, string accessToken = null)
        {
            var response = await SendFulcrumRequest(requestMethod, requestPath, payload, accessToken);
            return JsonConvert.DeserializeObject<T>(response);
        }


        public async Task<string> SendFulcrumRequest(HttpMethod requestMethod, string requestPath, object payload = null, string accessToken = null)
        {
            var jsonPayload = JsonConvert.SerializeObject(payload);
            return await SendFulcrumRequest(requestMethod, requestPath, jsonPayload, accessToken);
        }

        private async Task<string> SendFulcrumRequest(HttpMethod requestMethod, string requestUrl, string payload = null, string accessToken = null)
        {
            if (string.IsNullOrWhiteSpace(requestUrl))
            {
                throw new ArgumentException("Request URL cannot be null or empty.", nameof(requestUrl));
            }

            using var request = new HttpRequestMessage(requestMethod, requestUrl)
            {
                Content = !string.IsNullOrEmpty(payload)
                    ? new StringContent(payload, Encoding.UTF8, "application/json")
                    : null,
            };

            if (!string.IsNullOrEmpty(accessToken))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            }

            string responseBodyString = string.Empty;
            try
            {
                var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
                responseBodyString = await response.Content.ReadAsStringAsync();
                response.EnsureSuccessStatusCode();

                return responseBodyString;
            }
            catch (HttpRequestException err)
            {
                _logger.Error($"Failed to process request: {err.Message}; Fulcrum response: {responseBodyString}", err);
                throw new UserFriendlyException("Unable to process Request. Please try again later");
            }
        }

    }
}
