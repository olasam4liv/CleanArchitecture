

using SharedKernel.Model.Responses;
    using Microsoft.AspNetCore.Http;
    using System;
    using System.Threading.Tasks;
    namespace Web.Api.Middleware;

    /// <summary>
    /// Middleware for global exception handling to wrap technical failures as ResponseModel with 500 status code.
    /// </summary>
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly bool _isDevelopment;

        public GlobalExceptionMiddleware(RequestDelegate next, bool isDevelopment)
        {
            _next = next;
            _isDevelopment = isDevelopment;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                var problem = ResponseModel.Failure(
                    message: _isDevelopment ? ex.Message : "An unexpected error occurred",
                    responseCode: ResponseStatusCode.InternalServerError.ResponseCode);

                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                await context.Response.WriteAsJsonAsync(problem);
            }
        }
    }