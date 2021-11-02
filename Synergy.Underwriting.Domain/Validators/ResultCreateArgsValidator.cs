using FluentValidation;
using Synergy.Underwriting.Models;

namespace Synergy.Underwriting.Domain.Validators
{
    public class ResultCreateArgsValidator : AbstractValidator<ResultCreateArgs>
    {
        public ResultCreateArgsValidator()
        {
            this.RuleFor(x => x.BidNumber)
                .NotEmpty();

            this.RuleFor(x => x.ParcelId)
                .NotEmpty()
                .When(x => string.IsNullOrWhiteSpace(x.AdvertisementNumber));

            this.RuleFor(x => x.AdvertisementNumber)
                .NotEmpty()
                .When(x => string.IsNullOrWhiteSpace(x.ParcelId));

            this.RuleFor(x => x.TaxAmount)
                .GreaterThanOrEqualTo(0);

            this.RuleFor(x => x.Overbid)
                .GreaterThanOrEqualTo(0)
                .When(x => x.Overbid.HasValue);

            this.RuleFor(x => x.Premium)
                .GreaterThanOrEqualTo(0)
                .When(x => x.Premium.HasValue);

            this.RuleFor(x => x.InterestRate)
                .InclusiveBetween(0, 50);

            this.RuleFor(x => x.PenaltyRate)
                .InclusiveBetween(0, 50)
                .When(x => x.PenaltyRate.HasValue);

            this.RuleFor(x => x.RecoverableFees)
                .GreaterThanOrEqualTo(0)
                .When(x => x.RecoverableFees.HasValue);

            this.RuleFor(x => x.NonRecoverableFees)
                .GreaterThanOrEqualTo(0)
                .When(x => x.NonRecoverableFees.HasValue);
        }
    }
}
