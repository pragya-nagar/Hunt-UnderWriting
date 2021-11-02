using FluentValidation;
using Synergy.Underwriting.Models;

namespace Synergy.Underwriting.Domain.Validators
{
    public class ReviewReportArgsValidator : AbstractValidator<ReviewReportArgs>
    {
        public ReviewReportArgsValidator()
        {
            this.RuleFor(x => x.StateId).GreaterThan(0);
            this.RuleFor(x => x.SaleDateTo).NotNull().When(x => x.IsEventLocked == true);
            this.RuleFor(x => x.SaleDateFrom).NotNull().When(x => x.IsEventLocked == true);
        }
    }
}
