using FluentValidation;
using Synergy.Underwriting.Models.Bid;

namespace Synergy.Underwriting.Domain.Validators
{
    public class BidUpdateArgsValidator : AbstractValidator<BidUpdateArgs>
    {
        public BidUpdateArgsValidator()
        {
            this.RuleFor(x => x.Number)
                .NotEmpty();

            this.RuleFor(x => x.Entity)
                .NotEmpty();

            this.RuleFor(x => x.Portfolio)
                .NotEmpty();

            this.RuleFor(x => x.EventId)
                .NotEmpty();
        }
    }
}