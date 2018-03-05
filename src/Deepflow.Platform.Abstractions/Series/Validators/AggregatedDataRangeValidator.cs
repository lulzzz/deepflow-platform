using System;
using System.Collections.Generic;
using Deepflow.Platform.Abstractions.Series.Extensions;
using FluentValidation;

namespace Deepflow.Platform.Abstractions.Series.Validators
{
    public class AggregatedDataRangeValidator : AbstractValidator<AggregatedDataRange>
    {
        public AggregatedDataRangeValidator()
        {
            RuleFor(x => x.TimeRange)
                .Cascade(CascadeMode.StopOnFirstFailure)
                .NotNull().WithMessage("Time range must be not null")
                .SetValidator(new TimeRangeValidator())
                .Must(BeNonZeroLength).WithMessage("Time range must be non zero length for an aggregated data range");

            RuleFor(x => x.AggregationSeconds)
                .NotEmpty().WithMessage("Aggregation seconds must be non zero for an aggregated data range");

            var timeIndex = 0;
            var valueIndex = 0;
            RuleFor(x => x.Data)
                .Cascade(CascadeMode.StopOnFirstFailure)
                .NotNull().WithMessage("Data must be not null")
                .Must(BeEven).WithMessage("Size of data in aggregated data range must be an even number")
                .Must(BeValidTimes).WithMessage(dataRange => $"Timestamps in aggregated data range must be valid. Timestamp at index {timeIndex = GetInvalidTimeIndex(dataRange.Data)} is not valid. ({dataRange.Data[timeIndex]}, {dataRange.Data[timeIndex + 1]})")
                .Must(BeValidValues).WithMessage(dataRange => $"Values in aggregated data range must be valid. Value at index {valueIndex = GetInvalidValueIndex(dataRange.Data)} is not valid. ({dataRange.Data[valueIndex - 1]}, {dataRange.Data[valueIndex]})")
                .Must(BeInsideTimeRange).WithMessage("Data in aggregated data range must be inside time range and be at least one aggregation interval greater than the min time")
                .Must(BeInOrder).WithMessage("Data in raw aggregated range must be in order, not duplicated, and quantised to the aggregation seconds interval");
        }

        private bool BeNonZeroLength(TimeRange timeRange)
        {
            return timeRange.Min < timeRange.Max;
        }

        private bool BeEven(List<double> data)
        {
            return data.Count % 2 == 0;
        }

        private bool BeInOrder(AggregatedDataRange dataRange, List<double> doubles)
        {
            double lastTime = 0;
            foreach (var datum in dataRange.GetData())
            {
                if (datum.Time <= lastTime)
                {
                    return false;
                }

                if (Math.Abs(datum.Time % dataRange.AggregationSeconds) > 0.5)
                {
                    return false;
                }

                lastTime = datum.Time;
            }
            return true;
        }

        private bool BeInsideTimeRange(AggregatedDataRange dataRange, List<double> data)
        {
            var minTime = dataRange.TimeRange.Min + dataRange.AggregationSeconds;
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

        private bool BeValidTimes(AggregatedDataRange dataRange, List<double> data)
        {
            foreach (var datum in dataRange.GetData())
            {
                if (double.IsNaN(datum.Time))
                {
                    return false;
                }
            }

            return true;
        }

        private int GetInvalidTimeIndex(List<double> data)
        {
            for (var i = 0; i < data.Count; i += 2)
            {
                if (double.IsNaN(data[i]))
                {
                    return i;
                }
            }

            return -1;
        }

        private bool BeValidValues(AggregatedDataRange dataRange, List<double> data)
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

        private int GetInvalidValueIndex(List<double> data)
        {
            for (var i = 0; i < data.Count; i += 2)
            {
                if (double.IsNaN(data[i + 1]))
                {
                    return i + 1;
                }
            }

            return -1;
        }
    }
}
