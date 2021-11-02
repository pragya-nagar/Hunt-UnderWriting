using System.Collections.Generic;
using AutoMapper;
using Synergy.DataAccess.Entities;
using Synergy.Underwriting.DAL.Commands.Models;

namespace Synergy.Underwriting.DAL.Commands.MapProfiles
{
    public class AttachRulesToEventMapProfile : Profile
    {
        public AttachRulesToEventMapProfile()
        {
            CreateMap<AttachRulesToEventModel, EventDataCutStrategy>()
                .ConvertUsing((src, dst) =>
                {
                    var srcList = new List<EventDataCutRule>();
                    foreach (var rule in src.DataCutRuleIds)
                    {
                        srcList.Add(new EventDataCutRule { DataCutRuleId = rule });
                    }

                    return new EventDataCutStrategy
                    {
                        EventDataCutRules = srcList,
                        EventId = src.EventId,
                        IsActive = true,
                    };
                });
        }
    }
}
