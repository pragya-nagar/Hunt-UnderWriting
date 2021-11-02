using FluentValidation;
using Synergy.Underwriting.Models;
using Synergy.Underwriting.Models.Bid;

namespace Synergy.Underwriting.Domain.Validators
{
    public class BidCreateArgsValidator : AbstractValidator<BidCreateArgs>
    {
        public BidCreateArgsValidator()
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
