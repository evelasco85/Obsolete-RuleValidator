using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace RuleValidator_Proto.Integration.Utility
{
    public class SwapVisitor : ExpressionVisitor
    {
        readonly Expression _from, _to;

        public SwapVisitor(Expression from, Expression to)
        {
            this._from = from;
            this._to = to;
        }

        public override Expression Visit(Expression node)
        {
            return node == this._from ? this._to : base.Visit(node);
        }
    }
}
