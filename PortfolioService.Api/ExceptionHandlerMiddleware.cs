using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using System;

namespace PortfolioService.Api
{
    public class ExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlerMiddleware> _logger;

        public ExceptionHandlerMiddleware(RequestDelegate next, ILogger<ExceptionHandlerMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try {
                await _next(context);
            } catch (Exception ex) {
                _logger.LogError(ex, "Unhandled exception");

                context.Response.ContentType = "application/json";

                context.Response.StatusCode = ex switch {
                    ArgumentNullException => StatusCodes.Status400BadRequest,
                    ArgumentException => StatusCodes.Status400BadRequest,
                    KeyNotFoundException => StatusCodes.Status404NotFound,
                    _ => StatusCodes.Status500InternalServerError
                };

                var result = JsonSerializer.Serialize(new {
                    error = ex.Message
                });

                await context.Response.WriteAsync(result);
            }
        }
    }
}
