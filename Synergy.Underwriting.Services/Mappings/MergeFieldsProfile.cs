using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using AutoMapper;
using Synergy.ServiceBus.Messages;
using Synergy.Underwriting.DAL.Commands.Models.Results.MailMerge;

namespace Synergy.Underwriting.Services.Mappings
{
    public class MergeFieldsProfile : Profile
    {
        public MergeFieldsProfile()
        {
            this.CreateMap<List<MergeSingleFields>, MergeFields>(MemberList.None)

                .ForAllMembers(x =>
                {
                    var destPropToMap = x.DestinationMember;

                    var srcPropertyToMap = typeof(MergeSingleFields).GetProperty(destPropToMap.Name, BindingFlags.Instance | BindingFlags.Public);

                    var parameter = Expression.Parameter(typeof(MergeSingleFields), "source");
                    var propertyExpression = Expression.Property(parameter, srcPropertyToMap);
                    var conversion = Expression.Convert(propertyExpression, srcPropertyToMap.PropertyType);

                    var funcType = typeof(Func<,>).MakeGenericType(new Type[] { typeof(MergeSingleFields), srcPropertyToMap.PropertyType });
                    var lambdaMethod = typeof(Expression)
                        .GetMethod("Lambda", new Type[] { typeof(Type), typeof(Expression), typeof(ParameterExpression[]) });
                    var propertyAccessor = lambdaMethod.Invoke(null, new object[] { funcType, conversion, new ParameterExpression[] { parameter } });

                    var delegateExpression = typeof(Expression<>).MakeGenericType(new Type[] { funcType });
                    var compileMethod = delegateExpression.GetMethod("Compile", System.Type.EmptyTypes);
                    var compiledlambda = compileMethod.Invoke(propertyAccessor, null);

                    var selectorGenericMethod = typeof(Enumerable)
                        .GetMethods().Where(y => y.Name == "Select").FirstOrDefault(); // ("Select", new Type[] { typeof(IEnumerable<>), funcType });
                    var selectorMethod = selectorGenericMethod.MakeGenericMethod(new Type[] { typeof(MergeSingleFields), srcPropertyToMap.PropertyType });

                    x.MapFrom(sources => BuildArray(sources, selectorMethod, compiledlambda, srcPropertyToMap.PropertyType));
                });
        }

        public static object BuildArray(IEnumerable<MergeSingleFields> sources, MethodInfo selectorMethod, object compiledlambda, Type sourcePropertyType)
        {
            var enumerableResult = selectorMethod.Invoke(null, new object[] { sources, compiledlambda });
            var toArrayMethod = typeof(Enumerable).GetMethod("ToArray").MakeGenericMethod(new Type[] { sourcePropertyType });
            var result = toArrayMethod.Invoke(null, new object[] { enumerableResult });
            return result;
        }
    }
}
