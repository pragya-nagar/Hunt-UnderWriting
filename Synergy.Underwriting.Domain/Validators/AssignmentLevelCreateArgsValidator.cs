using FluentValidation;
using Synergy.Underwriting.Models.EventAssignment;

namespace Synergy.Underwriting.Domain.Validators
{
    public class AssignmentLevelCreateArgsValidator : AbstractValidator<AssignmentLevelCreateArgs>
    {
        public AssignmentLevelCreateArgsValidator()
        {
            this.RuleFor(x => x.Order)
                .GreaterThanOrEqualTo(0);
        }
    }
}
