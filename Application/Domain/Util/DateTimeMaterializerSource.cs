#region

using System;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

#endregion

namespace Application.Domain.Util
{
    public class DateTimeMaterializerSource : EntityMaterializerSource
    {
        private static readonly MethodInfo NormalizeMethod =
            typeof(DateTimeMapper).GetTypeInfo().GetMethod(nameof(DateTimeMapper.Normalize));

        private static readonly MethodInfo NormalizeNullableMethod =
            typeof(DateTimeMapper).GetTypeInfo().GetMethod(nameof(DateTimeMapper.NormalizeNullable));


        public override Expression CreateReadValueExpression(Expression valueBuffer, Type type, int index,
            IPropertyBase property = null)
        {
            if (type == typeof(DateTime))
            {
                return Expression.Call(
                    NormalizeMethod,
                    base.CreateReadValueExpression(valueBuffer, type, index, property)
                );
            }
            else if (type == typeof(DateTime?))
            {
                return Expression.Call(
                    NormalizeNullableMethod,
                    base.CreateReadValueExpression(valueBuffer, type, index, property)
                );
            }

            return base.CreateReadValueExpression(valueBuffer, type, index, property);
        }
    }

    public static class DateTimeMapper
    {
        public static DateTime Normalize(DateTime value)
        {
            return DateTime.SpecifyKind(value, DateTimeKind.Utc);
        }

        public static DateTime? NormalizeNullable(DateTime? value)
        {
            return value.HasValue ? DateTime.SpecifyKind(value.Value, DateTimeKind.Utc) : (DateTime?) null;
        }
    }
}