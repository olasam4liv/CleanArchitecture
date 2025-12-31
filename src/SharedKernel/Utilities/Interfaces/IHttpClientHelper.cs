using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using SharedKernel.Model.Responses;

namespace SharedKernel.Utilities.Interfaces;

public interface IHttpClientHelper
{
	Task<ResponseModel<T>> MakeAPIRequestAsync<T>(
		Uri url,
		HttpMethod method,
		object? payload = null,
		Dictionary<string, string>? headers = null,
		bool isForm = false,
		TimeSpan? timeout = null);
}
