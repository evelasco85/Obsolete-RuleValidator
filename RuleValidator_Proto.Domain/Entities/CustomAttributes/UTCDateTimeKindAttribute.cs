using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace RuleValidator_Proto.Domain.Entities.CustomAttributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class UTCDateTimeKindAttribute : Attribute
    {
        public const DateTimeKind Kind = DateTimeKind.Utc;

        public static void Apply(object entity)
        {
            if (entity == null)
                return;

            IEnumerable<PropertyInfo> properties = entity
                .GetType()
                .GetProperties()
                .Where(filter => (filter.PropertyType == typeof(DateTime)) || (filter.PropertyType == typeof(DateTime?)));

            IEnumerable<PropertyInfo> utcProperties = GetUTCProperties(entity);

            foreach(PropertyInfo property in properties)
            {
                UTCDateTimeKindAttribute attribute = utcProperties
                    .Where(filter => filter.Name == property.Name)
                    .Select(x => x.GetCustomAttribute<UTCDateTimeKindAttribute>())
                    .DefaultIfEmpty(null)
                    .FirstOrDefault();

                if (attribute == null)
                    continue;

                var dateTimeValue = (property.PropertyType == typeof(DateTime?)) ? (DateTime?)property.GetValue(entity) : (DateTime)property.GetValue(entity);

                if (dateTimeValue == null)
                    continue;

                property.SetValue(entity, GetUTCDateTime(dateTimeValue.Value));
            }
        }

        static DateTime GetUTCDateTime(DateTime dateTimeValue)
        {
            DateTime utcDateTime = DateTime.MinValue;

            if (dateTimeValue == null)
                return utcDateTime;

            utcDateTime = DateTime.SpecifyKind(dateTimeValue, UTCDateTimeKindAttribute.Kind);

            return utcDateTime;
        }

        static IEnumerable<PropertyInfo> GetUTCProperties(object entity)
        {
            IEnumerable<PropertyInfo> properties = new List<PropertyInfo>();

            CustomAttributeData customizedAttribute = entity
                .GetType()
                .BaseType
                .CustomAttributes
                .Where(filter => filter.AttributeType == typeof(MetadataTypeAttribute))
                .DefaultIfEmpty(null)
                .FirstOrDefault();

            if (customizedAttribute == null)
                return properties;

            Type metadataType = customizedAttribute
                .ConstructorArguments
                .Select(column => column.Value as Type)
                .DefaultIfEmpty(null)
                .FirstOrDefault();

            if (metadataType == null)
                return properties;

            properties = metadataType.GetProperties()
                .Where(filter => filter
                    .CustomAttributes
                    .Select(c => c.AttributeType)
                    .Contains(typeof(UTCDateTimeKindAttribute))
                    );

            return properties;
        }
    }
}
