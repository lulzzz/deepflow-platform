using System.Collections.Generic;
using Deepflow.Platform.Abstractions.Series.Extensions;
using FluentValidation;

namespace Deepflow.Platform.Abstractions.Series.Validators
{
    public class RawDataRangeValidator : AbstractValidator<RawDataRange>
    {
        public RawDataRangeValidator()
        {
            RuleFor(x => x.TimeRange)
                .Cascade(CascadeMode.StopOnFirstFailure)
                .NotNull().WithMessage("Time range must be not null")
                .SetValidator(new TimeRangeValidator());

            RuleFor(x => x.Data)
                .Cascade(CascadeMode.StopOnFirstFailure)
                .NotNull().WithMessage("Data must be not null")
                .Must(BeEven).WithMessage("Size of data in raw data range must be an even number")
                .Must(BeValidTimesAndValues).WithMessage("Values in raw data range must be valid")
                .Must(BeInsideTimeRange).WithMessage("Data in raw data range must be inside time range")
                .Must(BeInOrder).WithMessage("Data in raw time range must be in order and not duplicated");
        }

        private bool BeEven(List<double> data)
        {
            return data.Count % 2 == 0;
        }

        private bool BeInOrder(RawDataRange dataRange, List<double> doubles)
        {
            double lastTime = 0;
            foreach (var datum in dataRange.GetData())
            {
                if (datum.Time <= lastTime)
                {
                    return false;
                }

                lastTime = datum.Time;
            }
            return true;
        }

        private bool BeInsideTimeRange(RawDataRange dataRange, List<double> data)
        {
            if (dataRange.TimeRange == null)
            {
                return true;
            }

            var minTime = dataRange.TimeRange.Min;
            var maxTime = dataRange.TimeRange.Max;

            foreach (var datum in dataRange.GetData())
            {
                if (datum.Time < minTime)
                {
                    return false;
                }
                if (datum.Time > maxTime)
                {
                    return false;
                }
            }

            return true;
        }

        private bool BeValidTimesAndValues(RawDataRange dataRange, List<double> data)
        {
            foreach (var datum in dataRange.GetData())
            {
                if (double.IsNaN(datum.Value))
                {
                    return false;
                }
            }

            return true;
        }
    }
}