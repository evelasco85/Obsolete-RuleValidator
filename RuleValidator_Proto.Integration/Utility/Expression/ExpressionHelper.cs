using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RuleValidator_Proto.Integration.Utility
{
    public interface IExpressionHelper
    {
        Expression<Func<T, bool>> AnyOf<T>(params Expression<Func<T, bool>>[] expressions);
        TOut GetMemberExpressionValue<TIn, TOut>(TIn entity, string memberOrPropertyName);
    }
    public class ExpressionHelper : IExpressionHelper
    {
        static IExpressionHelper _helper;

        private ExpressionHelper() { }

        public static IExpressionHelper GetInstance()
        {
            if (_helper == null)
                _helper = new ExpressionHelper();

            return _helper;
        }

        public Expression<Func<T, bool>> AnyOf<T>(params Expression<Func<T, bool>>[] expressions)
        {
            if ((expressions == null) || (expressions.Length == 0))
                return x => false;

            Expression bodyExpr = expressions[0].Body;
            ParameterExpression bodyParam = expressions[0].Parameters.Single();

            for(int index = 0; index < expressions.Length; index++)
            {
                 Expression<Func<T, bool>> expression = expressions[index];
                 Expression swappedParam = new SwapVisitor(expression.Parameters.Single(), bodyParam).Visit(expression.Body);

                 bodyExpr = Expression.OrElse(bodyExpr, swappedParam);
            }

            return Expression.Lambda<Func<T, bool>>(bodyExpr, bodyParam);
        }

        public TOut GetMemberExpressionValue<TIn, TOut>(TIn entity, string memberOrPropertyName)
        {
            Expression ruleExpr = Expression.Constant(entity, typeof(TIn));
            MemberExpression currentMember = Expression.PropertyOrField(ruleExpr, memberOrPropertyName);
            TOut currentMemberValue = (TOut)(currentMember.Member as PropertyInfo).GetValue(entity);

            return currentMemberValue;
        }
    }
}
