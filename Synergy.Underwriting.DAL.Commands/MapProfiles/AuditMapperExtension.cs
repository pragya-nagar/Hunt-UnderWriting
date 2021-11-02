using System;
using AutoMapper;
using Synergy.DataAccess.Entities;

namespace Synergy.Underwriting.DAL.Commands.MapProfiles
{
    public static class AuditMapperExtension
    {
        public static IMappingExpression<TSource, TDest> IgnoreAuditMembers<TSource, TDest>(this IMappingExpression<TSource, TDest> expression)
            where TDest : IAuditEntity<Guid>
        {
            expression.ForMember(a => a.CreatedBy, src => src.Ignore())
                      .ForMember(a => a.CreatedById, src => src.Ignore())
                      .ForMember(a => a.CreatedOn, src => src.Ignore())
                      .ForMember(a => a.ModifiedBy, src => src.Ignore())
                      .ForMember(a => a.ModifiedById, src => src.Ignore())
                      .ForMember(a => a.ModifiedOn, src => src.Ignore())
                      .ForMember(a => a.DeletedOn, src => src.Ignore());
            return expression;
        }
    }
}
