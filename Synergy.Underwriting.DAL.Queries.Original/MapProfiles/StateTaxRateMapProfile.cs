using AutoMapper;
using Synergy.DataAccess.Abstractions;
using Synergy.DataAccess.Entities;
using Synergy.Underwriting.DAL.Queries.Original.Models;

namespace Synergy.Underwriting.DAL.Queries.Original.MapProfiles
{
    public class StateTaxRateMapProfile : Profile
    {
        public StateTaxRateMapProfile()
        {
            CreateMap<StateTaxe, StateTaxRateModel>()
                .ApplyAuditMembers();
        }
    }
}
