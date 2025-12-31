using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.Json;
using SharedKernel.Model.Responses;
using SharedKernel.Utilities.Interfaces;

namespace SharedKernel.Utilities;

internal sealed class HttpClientHelper(ILogger<HttpClientHelper> logger) : IHttpClientHelper
{
    private readonly ILogger<HttpClientHelper> _logger = logger;

    public async Task<ResponseModel<T>> MakeAPIRequestAsync<T>(string url, HttpMethod method, object? payload = null, Dictionary<string, string> headers = null, TimeSpan? timeout = null)
    {
        try
        {
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
            };

            using var client = new HttpClient(handler)
            {
                Timeout = timeout ?? TimeSpan.FromSeconds(60)
            };
            client.DefaultRequestHeaders.Connection.Add("keep-alive");

            if (headers?.Any() == true)
            {
                foreach (var header in headers)
                {
                    client.DefaultRequestHeaders.Add(header.Key, header.Value);
                }
            }

            var request = new HttpRequestMessage(method, url);

            if (payload != null)
            {
                request.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, ApiEncoding.Json);
            }

            var response = await client.SendAsync(request);
            //response.EnsureSuccessStatusCode();
            if (!response.IsSuccessStatusCode)
                return await Result<T>.FailAsync("request fail", statusCode: (int)response.StatusCode);

            string responseData = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<T>(responseData, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return await Result<T>.SuccessAsync(result, statusCode: (int)response.StatusCode);
        }
        catch (Exception ex)
        {

            _logger.LogError("External Service Exeption: {@ex}", ex);
            return await Result<T>.FailAsync("External Service failed");
        }

    }


    public RestResponse MakeXmlRequest(string url, string payload = null, Method method = Method.Post, List<PostHeaders> headers = null, bool isForm = false)
    {
        try
        {
            var response = new RestResponse();
            var options = new RestClientOptions(url);
            var client = new RestClient(options);
            options.RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErros) => true;
            var request = new RestRequest();
            request.Method = method;
            request.Timeout = TimeSpan.FromHours(1);
            request.AddHeader("Content-Type", "text/xml");

            if (headers is not null && headers.Count > 0)
            {
                foreach (var header in headers)
                {
                    request.AddHeader(header.Key, header.Value);
                }
            }

            if (payload is not null)
            {
                if (method == Method.Post || method == Method.Put)
                    request.AddParameter(ApiEncoding.Text, payload, ParameterType.RequestBody);
                response = client.Execute<dynamic>(request);
            }
            else
            {
                if (method == Method.Get)
                    request.AddParameter(isForm ? ApiEncoding.Form : ApiEncoding.Json, Newtonsoft.Json.JsonConvert.SerializeObject(payload), ParameterType.RequestBody);
                response = client.Execute(request);
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError("External Service Exeption: {@ex}", ex);
            return default;
        }

    }


}
