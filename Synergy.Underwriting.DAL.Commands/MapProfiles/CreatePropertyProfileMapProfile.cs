using System;
using System.Collections.Generic;
using AutoMapper;
using Synergy.DataAccess.Entities;
using Synergy.Underwriting.DAL.Commands.Models;

namespace Synergy.Underwriting.DAL.Commands.MapProfiles
{
    public class CreatePropertyProfileMapProfile : Profile
    {
        public CreatePropertyProfileMapProfile()
        {
            CreateMap<CreatePropertyProfileModel, PropertyProfile>()
                    .ConvertUsing((src, dst) =>
                    {
                        var propertyProfileRules = new List<PropertyProfileRulePropertyProfile>();
                        foreach (var ruleId in src.PropertyProfileRuleIds)
                        {
                            propertyProfileRules.Add(new PropertyProfileRulePropertyProfile { Id = Guid.NewGuid(), PropertyProfileRuleId = ruleId, PropertyProfileId = src.Id });
                        }

                        var profileStates = new List<PropertyProfileState>();
                        foreach (var stateId in src.StateIds)
                        {
                            profileStates.Add(new PropertyProfileState { Id = Guid.NewGuid(), StateId = stateId, PropertyProfileId = src.Id });
                        }

                        return new PropertyProfile
                        {
                            Id = src.Id,
                            PropertyProfileRulePropertyProfiles = propertyProfileRules,
                            PropertyProfileStates = profileStates,
                            IsActive = src.IsActive,
                            Name = src.Name,
                        };
                    });
        }
    }
}
