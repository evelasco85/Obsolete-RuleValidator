using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RuleValidator_Proto.Domain.Concrete
{
    public interface IAlteredRecordsRetriever<TEntity>
    {
        IAlteredRecordsRetriever<TEntity> SetEntities(Expression<Func<TEntity, object>> primaryKeyExpression, IEnumerable<TEntity> newEntityList, IEnumerable<TEntity> originalEntityList);
        IAlteredRecordsRetriever<TEntity> AddIgnoredProperty<TPropertyType>(Expression<Func<TEntity, TPropertyType>> property);
        IEnumerable<TEntity> GetAlteredOnlyEntities();
    }

    public class AlteredRecordsRetriever<TEntity> : IAlteredRecordsRetriever<TEntity>
    {
        IEnumerable<TEntity> _newEntityList;
        IEnumerable<TEntity> _originalEntityList;
        Expression<Func<TEntity, object>> _primaryKeyExpression;
        IList<string> _ignoredProperties;
        IList<PropertyInfo> _allProperties;

        private AlteredRecordsRetriever()
        {
            this._ignoredProperties = new List<string>();
            this._allProperties = typeof(TEntity)
                .GetProperties()
                .Where(property => 
                    !property.GetGetMethod().IsVirtual
                    )
                .ToArray();
        }

        static IAlteredRecordsRetriever<TEntity> _soleInstance;
        public static IAlteredRecordsRetriever<TEntity> GetInstance()
        {
            if (_soleInstance == null)
                _soleInstance = new AlteredRecordsRetriever<TEntity>();

            return _soleInstance;
        }

        public IAlteredRecordsRetriever<TEntity> SetEntities(Expression<Func<TEntity, object>> primaryKeyExpression, IEnumerable<TEntity> newEntityList, IEnumerable<TEntity> originalEntityList)
        {
            this._primaryKeyExpression = primaryKeyExpression;
            this._newEntityList = newEntityList;
            this._originalEntityList = originalEntityList;

            return this;
        }

        public IAlteredRecordsRetriever<TEntity> AddIgnoredProperty<TPropertyType>(Expression<Func<TEntity, TPropertyType>> property)
        {
            string propertyName = ((MemberExpression)property.Body).Member.Name;

            if ((!string.IsNullOrEmpty(propertyName)) && (!this._ignoredProperties.Contains(propertyName)))
                this._ignoredProperties.Add(propertyName);

            return this;
        }

        public IEnumerable<TEntity> GetAlteredOnlyEntities()
        {
            IList<TEntity> alteredRecords = new List<TEntity>();

            if (this._newEntityList == null)
                return alteredRecords;

            if (this._originalEntityList == null)
                return this._newEntityList;

            foreach(TEntity entity in this._newEntityList)
            {
                object primaryKeyValue = this._primaryKeyExpression.Compile().Invoke(entity);

                Expression<Func<TEntity, bool>> whereCondition = Expression
                    .Lambda<Func<TEntity, bool>>(
                        Expression.Equal(
                            (this._primaryKeyExpression.Body as UnaryExpression).Operand,
                            Expression.Constant(primaryKeyValue, (this._primaryKeyExpression.Body as UnaryExpression).Operand.Type)),
                        this._primaryKeyExpression.Parameters
                        );

                TEntity originalEntity = this._originalEntityList
                    .AsQueryable()
                    .Where(whereCondition)
                    .DefaultIfEmpty(default(TEntity))
                    .FirstOrDefault();

                if (!this.ContainExactDetails(this._allProperties.Where(property => !this._ignoredProperties.Any(propertyName => propertyName == property.Name)), entity, originalEntity))
                    alteredRecords.Add(entity);
            }

            return alteredRecords;
        }

        bool ContainExactDetails(IEnumerable<PropertyInfo> properties, TEntity alteredEntity, TEntity baseEntity)
        {
            bool exactRecord = true;

            if ((alteredEntity == null) && (baseEntity == null))      //Both entity does not exist
                return exactRecord;

            if ((alteredEntity == null) || (baseEntity == null))      //Only one of the entities exists
                return false;

            foreach(PropertyInfo property in properties)
            {
                object leftValue = property.GetValue(alteredEntity);
                object rightValue = property.GetValue(baseEntity);

                if (
                    ((leftValue == null) && (rightValue == null))
                    )
                    continue;

                 if(
                     ((leftValue == null) && (rightValue != null)) ||
                     ((leftValue != null) && (rightValue == null))
                   )
                 {
                     exactRecord = false;
                     break;
                 }

                if(!leftValue.Equals(rightValue))
                {
                    exactRecord = false;
                    break;
                }
            }

            return exactRecord;
        }
    }
}
