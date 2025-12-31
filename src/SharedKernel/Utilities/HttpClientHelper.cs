using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging;
using SharedKernel.Model.Responses;
using SharedKernel.Utilities.Interfaces;

namespace SharedKernel.Utilities;

internal sealed class HttpClientHelper(
    ILogger<HttpClientHelper> logger,
    IHttpClientFactory httpClientFactory) : IHttpClientHelper
{
    private readonly ILogger<HttpClientHelper> _logger = logger;
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private static bool IsWriteMethod(HttpMethod method) =>
        method == HttpMethod.Post || method == HttpMethod.Put || method == HttpMethod.Patch;

    public Task<ResponseModel<T>> MakeAPIRequestAsync<T>(
        string url,
        HttpMethod method,
        object? payload = null,
        Dictionary<string, string>? headers = null,
        bool isForm = false,
        TimeSpan? timeout = null) =>
        Uri.TryCreate(url, UriKind.Absolute, out Uri? requestUri)
            ? MakeApiRequestInternal<T>(requestUri, method, payload, headers, isForm, timeout)
            : Task.FromResult(ResponseModel<T>.Failure("Invalid URL"));

    public Task<ResponseModel<T>> MakeAPIRequestAsync<T>(
        Uri url,
        HttpMethod method,
        object? payload = null,
        Dictionary<string, string>? headers = null,
        bool isForm = false,
        TimeSpan? timeout = null) =>
        MakeApiRequestInternal<T>(url, method, payload, headers, isForm, timeout);

    private async Task<ResponseModel<T>> MakeApiRequestInternal<T>(
        Uri url,
        HttpMethod method,
        object? payload,
        Dictionary<string, string>? headers,
        bool isForm,
        TimeSpan? timeout)
    {
        try
        {
            using HttpClient client = _httpClientFactory.CreateClient(nameof(HttpClientHelper));
            client.Timeout = timeout ?? TimeSpan.FromSeconds(60);
            client.DefaultRequestHeaders.Connection.Add("keep-alive");

            if (headers?.Any() == true)
            {
                foreach (KeyValuePair<string, string> header in headers)
                {
                    client.DefaultRequestHeaders.Add(header.Key, header.Value);
                }
            }

            using HttpRequestMessage request = new(method, url);

            if (payload is not null && IsWriteMethod(method))
            {
                request.Content = isForm && payload is Dictionary<string, string> formData
                    ? new FormUrlEncodedContent(formData)
                    : new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            }

            HttpResponseMessage response = await client.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                return ResponseModel<T>.Failure($"Request failed with status code {(int)response.StatusCode}");
            }

            string responseData = await response.Content.ReadAsStringAsync();
            T? result = JsonSerializer.Deserialize<T>(responseData, JsonOptions);

            return result is null
                ? ResponseModel<T>.Failure("Unable to parse response")
                : ResponseModel<T>.Success(result, "Request successful");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "External service exception");
            return ResponseModel<T>.Failure("External service failed");
        }

    }


    public Task<ResponseModel<string>> MakeXmlRequestAsync(
        string url,
        HttpMethod method,
        string? payload = null,
        Dictionary<string, string>? headers = null,
        TimeSpan? timeout = null) =>
        Uri.TryCreate(url, UriKind.Absolute, out Uri? requestUri)
            ? MakeXmlRequestInternal(requestUri, method, payload, headers, timeout)
            : Task.FromResult(ResponseModel<string>.Failure(message: "Invalid URL"));

    public Task<ResponseModel<string>> MakeXmlRequestAsync(
        Uri url,
        HttpMethod method,
        string? payload = null,
        Dictionary<string, string>? headers = null,
        TimeSpan? timeout = null) =>
        MakeXmlRequestInternal(url, method, payload, headers, timeout);

    private async Task<ResponseModel<string>> MakeXmlRequestInternal(
        Uri url,
        HttpMethod method,
        string? payload,
        Dictionary<string, string>? headers,
        TimeSpan? timeout)
    {
        try
        {
            using HttpClient client = _httpClientFactory.CreateClient();
            client.Timeout = timeout ?? TimeSpan.FromSeconds(60);
            client.DefaultRequestHeaders.Connection.Add("keep-alive");

            if (headers?.Any() == true)
            {
                foreach (KeyValuePair<string, string> header in headers)
                {
                    client.DefaultRequestHeaders.Add(header.Key, header.Value);
                }
            }

            using HttpRequestMessage request = new(method, url);
            request.Headers.TryAddWithoutValidation("Content-Type", "text/xml");

            if (payload is not null && IsWriteMethod(method))
            {
                request.Content = new StringContent(payload, Encoding.UTF8, "text/xml");
            }

            HttpResponseMessage response = await client.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                return ResponseModel<string>.Failure(
                    message: $"Request failed with status code {(int)response.StatusCode}");
            }

            string responseData = await response.Content.ReadAsStringAsync();
            return ResponseModel<string>.Success(responseData, "Request successful");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "External XML service exception");
            return ResponseModel<string>.Failure(message: "External service failed");
        }
    }


}
