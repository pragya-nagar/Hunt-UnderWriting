using System;
using AutoMapper;
using Synergy.DataAccess.Entities;
using Synergy.Underwriting.DAL.Commands.Models;

namespace Synergy.Underwriting.DAL.Commands.MapProfiles
{
    public class CreatePropertyProfileDelinquencyModelMapProfile : Profile
    {
        public CreatePropertyProfileDelinquencyModelMapProfile()
        {
            CreateMap<CreatePropertyProfileDelinquencyModel, PropertyProfileDelinquency>()
                .IgnoreAuditMembers()
                .ForMember(c => c.Id, src => src.MapFrom(exp => Guid.NewGuid()))
                .ForMember(c => c.Delinquency, src => src.Ignore())
                .ForMember(c => c.PropertyProfile, src => src.Ignore())
                .ForMember(c => c.DelinquencyId, src => src.MapFrom(exp => exp.DelinquencyId))
                .ForMember(c => c.PropertyProfileId, src => src.MapFrom(exp => exp.PropertyProfileId))
                ;
        }
    }
}
