using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using SharedKernel.Model.Responses;

namespace SharedKernel.Utilities.Interfaces;

public interface IHttpClientHelper
{
	Task<ResponseModel<T>> MakeAPIRequestAsync<T>(
		string url,
		HttpMethod method,
		object? payload = null,
		Dictionary<string, string>? headers = null,
		bool isForm = false,
		TimeSpan? timeout = null);

	Task<ResponseModel<T>> MakeAPIRequestAsync<T>(
		Uri url,
		HttpMethod method,
		object? payload = null,
		Dictionary<string, string>? headers = null,
		bool isForm = false,
		TimeSpan? timeout = null);

	Task<ResponseModel<string>> MakeXmlRequestAsync(
		string url,
		HttpMethod method,
		string? payload = null,
		Dictionary<string, string>? headers = null,
		TimeSpan? timeout = null);

	Task<ResponseModel<string>> MakeXmlRequestAsync(
		Uri url,
		HttpMethod method,
		string? payload = null,
		Dictionary<string, string>? headers = null,
		TimeSpan? timeout = null);
}
