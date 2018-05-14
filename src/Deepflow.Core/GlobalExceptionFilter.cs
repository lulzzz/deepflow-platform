using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace Deepflow.Core
{
    public abstract class GlobalExceptionFilter : IExceptionFilter
    {
        private readonly ILogger<GlobalExceptionFilter> _logger;

        private static readonly IDictionary<Type, Func<Exception, ObjectResult>> ExceptionHandlers = new Dictionary<Type, Func<Exception, ObjectResult>>
        {
            {typeof(FluentValidation.ValidationException), HandleFluentValidationException}
        };

        protected abstract IDictionary<Type, Func<Exception, ObjectResult>> CustomExceptionHandlers { get; set; }

        protected GlobalExceptionFilter(ILogger<GlobalExceptionFilter> logger)
        {
            _logger = logger;
        }

        public void OnException(ExceptionContext context)
        {
            HandleException(context);
        }
        
        protected virtual void HandleException(ExceptionContext context)
        {
            var exception = context.Exception;
            var exceptionType = exception.GetType();

            var allExceptionHandlers = ExceptionHandlers.Concat(CustomExceptionHandlers).ToDictionary(x => x.Key, x => x.Value);

            if (allExceptionHandlers.ContainsKey(exceptionType))
            {
                // If we have an exception handler registered for this exception type let's use it
                context.Result = allExceptionHandlers[exceptionType](exception);
            }
            else
            {
                // We have no registered exception handler so 
                context.Result = HandleUnhandledException(exception);
            }

            _logger.LogError(context.Exception, "Failed processing request");
        }

        private static ObjectResult HandleFluentValidationException(Exception exception)
        {
            return CreateErrorResponse<BadRequestObjectResult>(ErrorResponseFactory.CreateValidation((FluentValidation.ValidationException)exception));
        }

        private ObjectResult HandleUnhandledException(Exception exception)
        {
            return CreateErrorResponse<ObjectResult>(ErrorResponseFactory.CreateUnhandled(exception), 500);
        }

        protected static TResult CreateErrorResponse<TResult>(Object errorResponse, int? statusCode = null) where TResult : ObjectResult
        {
            var result = (TResult)Activator.CreateInstance(typeof(TResult), new[] { errorResponse });
            result.DeclaredType = typeof(ErrorResponse);
            if (statusCode.HasValue)
            {
                result.StatusCode = statusCode.Value;
            }

            return result;
        }
    }
}