using AutoMapper;
using Synergy.DataAccess.Entities;
using Synergy.Underwriting.DAL.Commands.Models;

namespace Synergy.Underwriting.DAL.Commands.MapProfiles
{
    public class CreateOperationStatusModelMapProfile : Profile
    {
        public CreateOperationStatusModelMapProfile()
        {
            this.CreateMap<CreateOperationStatusModel, OperationStatus>()
                .IgnoreAuditMembers();
        }
    }
}
