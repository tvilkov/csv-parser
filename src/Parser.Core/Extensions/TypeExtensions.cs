using System;
using System.Linq.Expressions;

namespace Parser.Core.Extensions
{
    public static class TypeExtensions
    {
        public static object GetDefaultValue(this Type type)
        {
            return type.IsValueType ? Activator.CreateInstance(type) : null;
        }

        public static Action<object, object> CreateSetter(this Type containerType, string propOrFieldName)
        {
            if (containerType == null) throw new ArgumentNullException("containerType");
            if (string.IsNullOrWhiteSpace(propOrFieldName)) throw new ArgumentNullException("propOrFieldName");

            var expContainerParam = Expression.Parameter(typeof(object), "container");
            var expValueParam = Expression.Parameter(typeof(object), "value");
            var expPropOrField = Expression.PropertyOrField(Expression.Convert(expContainerParam, containerType), propOrFieldName);

            var expSet = Expression.Assign(expPropOrField, Expression.Convert(expValueParam, expPropOrField.Type));

            return Expression.Lambda<Action<object, object>>(expSet, expContainerParam, expValueParam).Compile();
        }
    }
}