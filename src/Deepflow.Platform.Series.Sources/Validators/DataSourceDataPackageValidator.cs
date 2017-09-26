using Deepflow.Platform.Abstractions.Series.Validators;
using Deepflow.Platform.Abstractions.Sources;
using FluentValidation;

namespace Deepflow.Platform.Series.Sources.Validators
{
    public class DataSourceDataPackageValidator : AbstractValidator<DataSourceDataPackage>
    {
        public DataSourceDataPackageValidator()
        {
            RuleFor(x => x.AggregatedRange)
                .NotEmpty()
                .SetValidator(new AggregatedDataRangeValidator());
        }
    }
}
