using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RuleValidator_Proto.Integration.Utility
{
    public interface IDynamicFilterBuilder<T> where T : class
    {
        ParameterExpression QueryableType { get; }

        IDynamicFilterBuilder<T> AddCustomizedFilterToCurrentQuery(Expression conditionExpression);
        IQueryable<T> GetQuery();
        ParameterExpression GetParameterExpressionInstance<T>(string parameterName);
        void UpdateInternalQueryData(IQueryable<T> query);
    }
    public class DynamicFilterBuilder<T> : IDynamicFilterBuilder<T> where T : class
    {
        IQueryable<T> _query;
        MethodCallExpression _existingWhereExpression;
        Expression<Func<T, bool>> _existingCondition;
        ParameterExpression _queryableType;

        public ParameterExpression QueryableType { get { return _queryableType; } }

        public DynamicFilterBuilder(IQueryable<T> query)
        {
            this.UpdateInternalQueryData(query);
        }

        public void UpdateInternalQueryData(IQueryable<T> query)
        {
            this._query = query;

            this.UpdateExpressionTree();
        }

        void UpdateExpressionTree()
        {
            _existingWhereExpression = this.FindWhereMethod(_query.Expression as MethodCallExpression);                  //Get existing filter if any
            _existingCondition = this.GetExistingConditionExpression(_existingWhereExpression);                           //Create member expression used in filtering
            _queryableType = this.GetQueryableType(_existingCondition);
        }

        MethodCallExpression FindWhereMethod(MethodCallExpression expression)       //Iterate the expression tree recursively and locate the where statement
        {
            if (expression == null)
                return null;

            if (expression.Method.Name == "Where")
                return expression;

            foreach (Expression sub in expression.Arguments)
            {
                MethodCallExpression result = FindWhereMethod(sub as MethodCallExpression);
                if (result != null)
                    return result;
            }

            return null;
        }

        Expression<Func<T, bool>> GetExistingConditionExpression(MethodCallExpression existingWhereExpression)
        {
            Expression<Func<T, bool>> existingCondition = (existingWhereExpression == null) ? 
                null : 
                ((existingWhereExpression.Arguments[1] as UnaryExpression).Operand as Expression<Func<T, bool>>);

            return existingCondition;
        }

        ParameterExpression GetQueryableType(Expression<Func<T, bool>> existingCondition)
        {
            ParameterExpression entityParam = ((existingCondition != null) && (existingCondition.Parameters.Count > 0)) ? existingCondition.Parameters[0] : Expression.Parameter(typeof(T), string.Empty);

            return entityParam;
        }

        public ParameterExpression GetParameterExpressionInstance<T>(string parameterName)
        {
            ParameterExpression queryableType = Expression.Parameter(typeof(T), parameterName);

            return queryableType;
        }

        BinaryExpression CreateConditionExpression(Expression<Func<T, bool>> existingCondition, MemberExpression propertyMember, ConstantExpression conditionValue)
        {
            BinaryExpression newCondition = Expression.Equal(propertyMember, conditionValue);

            BinaryExpression conditionExpression = this.CreateConditionExpression(existingCondition, newCondition);

            return conditionExpression;
        }

        BinaryExpression CreateConditionExpression(Expression<Func<T, bool>> existingCondition, BinaryExpression condition)
        {
            BinaryExpression newCondition = condition;

            BinaryExpression conditionExpression = (existingCondition != null)
                ? Expression.AndAlso(existingCondition.Body, newCondition)                             //Merge existing and new filter expression
                : newCondition;

            return conditionExpression;
        }

        MethodCallExpression CreateWhereQueryCall<TQueryType, TExpressionInType>(IQueryable<TQueryType> existingQuery, Expression<Func<TExpressionInType, bool>> conditionLambda)
        {
            MethodCallExpression call = Expression.Call(
                typeof(Queryable),
                "Where",                                            //'Where' method of IQueryable
                new Type[] { existingQuery.ElementType },
                existingQuery.Expression,                                //Main expression
                conditionLambda                                           //Filter expression
                );

            return call;
        }

        public IDynamicFilterBuilder<T> AddCustomizedFilterToCurrentQuery(Expression conditionExpression)
        {
            Expression<Func<T, bool>> lambda = Expression.Lambda<Func<T, bool>>(conditionExpression, QueryableType);

            MethodCallExpression call = this.CreateWhereQueryCall(this._query, lambda);

            this._query = _query.Provider.CreateQuery<T>(call);

            this.UpdateExpressionTree();

            return this;
        }

        public IQueryable<T> GetQuery()
        {
            return this._query;
        }
    }

}
