using System;
using System.Collections.Generic;
using AutoMapper;
using Synergy.DataAccess.Entities;
using Synergy.Underwriting.DAL.Commands.Models;

namespace Synergy.Underwriting.DAL.Commands.MapProfiles
{
    public class UpdatePropertyProfileMapProfile : Profile
    {
        public UpdatePropertyProfileMapProfile()
        {
            CreateMap<UpdatePropertyProfileModel, PropertyProfile>()
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

                       dst.Id = src.Id;
                       dst.PropertyProfileRulePropertyProfiles = propertyProfileRules;
                       dst.PropertyProfileStates = profileStates;
                       dst.IsActive = src.IsActive;
                       dst.Name = src.Name;

                       return dst;
                   });
        }
    }
}
