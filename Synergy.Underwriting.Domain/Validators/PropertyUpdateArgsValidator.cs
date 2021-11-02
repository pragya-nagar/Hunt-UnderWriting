using FluentValidation;
using Synergy.Underwriting.Models.Property;

namespace Synergy.Underwriting.Domain.Validators
{
    public class PropertyUpdateArgsValidator : AbstractValidator<PropertyUpdateArgs>
    {
        public PropertyUpdateArgsValidator()
        {
            this.RuleFor(x => x.GeneralLandUseCodeId)
                .GreaterThan(0);

            this.RuleFor(x => x.InternalLandUseCodeId)
                .GreaterThan(0);
        }
    }
}
