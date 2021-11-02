using FluentValidation;
using Synergy.Underwriting.Models;

namespace Synergy.Underwriting.Domain.Validators
{
    public class EventCreateArgsValidator : AbstractValidator<EventCreateArgs>
    {
        public EventCreateArgsValidator()
        {
            this.RuleFor(x => x.AssignedToUserId)
                .NotEmpty();

            this.RuleFor(x => x.SaleDate)
                .NotEmpty();

            this.RuleFor(x => x.Type)
                .IsInEnum();

            this.RuleFor(x => x.AuctionType)
                .IsInEnum();

            this.RuleFor(x => x.SaleDateStatus)
                .IsInEnum();

            this.RuleFor(x => x.FinalPaymentType)
                .IsInEnum();

            this.RuleFor(x => x.InterestRate)
                .GreaterThanOrEqualTo(0)
                .LessThanOrEqualTo(100)
                .When(x => x.InterestRate.HasValue);

            this.RuleFor(x => x.CountyId)
                .Must(x => x.HasValue)
                .When(x => string.IsNullOrWhiteSpace(x.CountyName) == true);

            this.RuleFor(x => x.CountyName)
                .NotEmpty()
                .When(x => x.CountyId.HasValue == false);

            this.RuleFor(x => x.StateId)
                .NotEmpty();
        }
    }
}
