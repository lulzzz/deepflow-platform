using System;
using FluentValidation;
using Newtonsoft.Json;

namespace Deepflow.Platform.Abstractions.Series.Validators
{
    public class TimeRangeValidator : AbstractValidator<TimeRange>
    {
        private static readonly long MaxAcceptableTime = (long) (new DateTime(2100, 1, 1) - new DateTime(1970, 1, 1)).TotalSeconds;
        private static readonly long MinAcceptableTime = 0;

        public TimeRangeValidator()
        {
            RuleFor(x => x.Min)
                .Cascade(CascadeMode.StopOnFirstFailure)
                .NotEmpty()
                .GreaterThan(MinAcceptableTime)
                .WithMessage(timeRange => $"Time range has a zero min time. {JsonConvert.SerializeObject(timeRange)}");

            RuleFor(x => x.Max)
                .Cascade(CascadeMode.StopOnFirstFailure)
                .NotEmpty()
                .LessThan(MaxAcceptableTime)
                .WithMessage(timeRange => $"Time range has a too high max time. {JsonConvert.SerializeObject(timeRange)}");

            RuleFor(x => x)
                .Must(BeInOrder).WithMessage("Time range min must be below or equal to max");
        }

        private bool BeInOrder(TimeRange timeRange)
        {
            return timeRange.Min <= timeRange.Max;
        }
    }
}
