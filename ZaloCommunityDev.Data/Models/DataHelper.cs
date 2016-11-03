using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace ZaloCommunityDev.Data.Models
{

    internal static class DataHelper
    {
        public static string TableName<T>() where T : class
            => typeof(T).GetAttributeValue<TableAttribute, string>(x => x.Name);

        public static TValue GetAttributeValue<TAttribute, TValue>(this Type type, Func<TAttribute, TValue> valueSelector) where TAttribute : Attribute
        {
            var att = type.GetCustomAttributes(
                typeof(TAttribute), true
            ).FirstOrDefault() as TAttribute;
            if (att != null)
            {
                return valueSelector(att);
            }
            return default(TValue);
        }


        public static PropertyInfo GetProperty<TEntity, TProperty>(this Expression<Func<TEntity, TProperty>> source)
        {

            var member = source.Body as MemberExpression;

            if (member == null)
            {
                throw new ArgumentException(string.Format("Expression '{0}' refers to a method, not a property.", source));
            }

            var propertyInfo = member.Member as PropertyInfo;

            if (propertyInfo == null)
            {
                throw new ArgumentException(string.Format(
                    "Expression '{0}' refers to a field, not a property.", source));
            }

            return propertyInfo;
        }

        public static string GetPropertyName<TEntity, TProperty>(this Expression<Func<TEntity, TProperty>> source) => source.GetProperty().Name;
    }
}