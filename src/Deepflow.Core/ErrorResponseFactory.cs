using System;
using System.Linq;
using FluentValidation;

namespace Deepflow.Core
{
    public static class ErrorResponseFactory
    {
        public static ValidationErrorResponse CreateValidation(ValidationException exception)
        {
            var error = CreateValidationErrorResponse();
            error.Errors = exception.Errors.Select(e => new ValidationErrorResponse.Item { FieldName = e.PropertyName, Description = e.ErrorMessage });
            return error;
        }

        private static ValidationErrorResponse CreateValidationErrorResponse()
        {
            return new ValidationErrorResponse("Failed Validation", "The following item(s) failed validation");
        }

        public static ErrorResponse CreateUnhandled(Exception exception)
        {
            return new ErrorResponse("Unhandled Exception", exception.Message);
        }
    }
}