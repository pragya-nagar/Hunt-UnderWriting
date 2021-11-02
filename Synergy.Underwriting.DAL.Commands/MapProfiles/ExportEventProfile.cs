using AutoMapper;
using Synergy.DataAccess.Entities;
using Synergy.Underwriting.DAL.Commands.Models.Results;

namespace Synergy.Underwriting.DAL.Commands.MapProfiles
{
    public class ExportEventProfile : Profile
    {
        public ExportEventProfile()
        {
            this.CreateMap<Event, ExportEventModel>()
                .ForMember(x => x.County, x => x.MapFrom(e => e.County.Name))
                .ForMember(x => x.State, x => x.MapFrom(e => e.State.Name))
                .ForMember(x => x.EventType, x => x.MapFrom(e => e.EventType.Name))
                .ForMember(x => x.FinalPaymentType, x => x.MapFrom(e => e.FinalPaymentType.Description))
                .ForMember(x => x.AuctionType, x => x.MapFrom(e => e.AuctionType.Description))
                .ForMember(x => x.SaleDateStatus, x => x.MapFrom(e => e.SaleDateStatus.Description))

                .ForMember(x => x.DepositAmount, x => x.MapFrom(e => e.DepositAmount == null ? 0 : e.DepositAmount.Value))
                .ForMember(x => x.TreasurerFee, x => x.MapFrom(e => e.TreasurerFee == null ? 0 : e.TreasurerFee.Value))
                .ForMember(x => x.InterestRate, x => x.MapFrom(e => e.InterestRate == null ? 0 : e.InterestRate.Value))
                .ForMember(x => x.EstimatedDepositAmount, x => x.MapFrom(e => e.EstimatedDepositAmount == null ? 0 : e.EstimatedDepositAmount.Value))
                .ForMember(x => x.EstimatedPurchaseAmount, x => x.MapFrom(e => e.EstimatedPurchaseAmount == null ? 0 : e.EstimatedPurchaseAmount.Value))
                .ForMember(x => x.RefundAmount, x => x.MapFrom(e => e.RefundAmount == null ? 0 : e.RefundAmount.Value))
                ;
        }
    }
}