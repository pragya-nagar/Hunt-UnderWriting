using AutoMapper;
using Synergy.DataAccess.Entities;
using Synergy.Underwriting.DAL.Commands.Models;

namespace Synergy.Underwriting.DAL.Commands.MapProfiles
{
    public class UpdateOperationStatusModelMapProfile : Profile
    {
        public UpdateOperationStatusModelMapProfile()
        {
            this.CreateMap<UpdateOperationStatusModel, OperationStatus>()
                .IgnoreAuditMembers();
        }
    }
}
