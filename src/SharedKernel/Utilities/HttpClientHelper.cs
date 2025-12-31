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

    public async Task<ResponseModel<T>> MakeAPIRequestAsync<T>(
        Uri url,
        HttpMethod method,
        object? payload = null,
        Dictionary<string, string>? headers = null,
        bool isForm = false,
        TimeSpan? timeout = null)
    {
        try
        {
            using HttpClient client = _httpClientFactory.CreateClient();
            client.Timeout = timeout ?? TimeSpan.FromSeconds(60);
            client.DefaultRequestHeaders.Connection.Add("keep-alive");


            // Add custom headers
            if (headers?.Any() == true)
            {
                foreach (KeyValuePair<string, string> header in headers)
                {
                    client.DefaultRequestHeaders.Add(header.Key, header.Value);
                }
            }

            using HttpRequestMessage request = new(method, url);
            
            // Add payload for POST, PUT, PATCH
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


    // public RestResponse MakeXmlRequest(string url, string payload = null, Method method = Method.Post, List<PostHeaders> headers = null, bool isForm = false)
    // {
    //     try
    //     {
    //         var response = new RestResponse();
    //         var options = new RestClientOptions(url);
    //         var client = new RestClient(options);
    //         options.RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErros) => true;
    //         var request = new RestRequest();
    //         request.Method = method;
    //         request.Timeout = TimeSpan.FromHours(1);
    //         request.AddHeader("Content-Type", "text/xml");

    //         if (headers is not null && headers.Count > 0)
    //         {
    //             foreach (var header in headers)
    //             {
    //                 request.AddHeader(header.Key, header.Value);
    //             }
    //         }

    //         if (payload is not null)
    //         {
    //             if (method == Method.Post || method == Method.Put)
    //                 request.AddParameter(ApiEncoding.Text, payload, ParameterType.RequestBody);
    //             response = client.Execute<dynamic>(request);
    //         }
    //         else
    //         {
    //             if (method == Method.Get)
    //                 request.AddParameter(isForm ? ApiEncoding.Form : ApiEncoding.Json, Newtonsoft.Json.JsonConvert.SerializeObject(payload), ParameterType.RequestBody);
    //             response = client.Execute(request);
    //         }

    //         return response;
    //     }
    //     catch (Exception ex)
    //     {
    //         _logger.LogError("External Service Exeption: {@ex}", ex);
    //         return default;
    //     }

    // }


}
