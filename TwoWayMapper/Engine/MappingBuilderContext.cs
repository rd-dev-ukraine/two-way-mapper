using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using TwoWayMapper.Conversion;

namespace TwoWayMapper.Engine
{
    public class MappingBuilderContext
    {
        private int variableCounter = 0;

        public MappingBuilderContext(object[] converters)
        {
            if (converters == null)
                throw new ArgumentNullException(nameof(converters));

            Converters = converters;
            IndexerParameters = new Stack<ParameterExpression>();
            ContextParameter = Expression.Parameter(typeof(MappingContext), "context");
        }

        public Stack<ParameterExpression> IndexerParameters { get; }

        public ParameterExpression ContextParameter { get; }

        public object[] Converters { get; }

        public ParameterExpression CreateIteratorVariable()
        {
            variableCounter++;
            return Expression.Parameter(typeof(int), $"var{variableCounter}");
        }

        /// <summary>
        /// Rewrite given expression to use indexer parameters in it indexer
        /// </summary>
        /// <param name="expr"></param>
        /// <returns></returns>
        public Expression Rewrite(Expression expr)
        {
            if (expr == null)
                throw new ArgumentNullException(nameof(expr));

            var visitor = new RewriteIndexerExpressionVisitor(this);
            var result = visitor.Visit(expr);
            return result;
        }
        

        private class RewriteIndexerExpressionVisitor : ExpressionVisitor
        {
            private readonly MappingBuilderContext context;

            public RewriteIndexerExpressionVisitor(MappingBuilderContext context)
            {
                this.context = context;
            }

            protected override Expression VisitIndex(IndexExpression node)
            {
                var num = ExpressionHelper.GetIndexerCount(node);

                var index = context.IndexerParameters.ElementAt(num);

                return Expression.MakeIndex(Visit(node.Object), node.Indexer, new[] { index });
            }

            protected override Expression VisitBinary(BinaryExpression node)
            {
                var num = ExpressionHelper.GetIndexerCount(node);
                var index = context.IndexerParameters.ElementAt(context.IndexerParameters.Count - num);

                if (node.NodeType == ExpressionType.ArrayIndex)
                    return Expression.ArrayIndex(Visit(node.Left), index);

                return base.VisitBinary(node);
            }

            /// <summary>
            /// Visits the children of the <see cref="T:System.Linq.Expressions.MethodCallExpression"/>.
            /// </summary>
            /// <returns>
            /// The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.
            /// </returns>
            /// <param name="node">The expression to visit.</param>
            protected override Expression VisitMethodCall(MethodCallExpression node)
            {
                var num = ExpressionHelper.GetIndexerCount(node);
                var index = context.IndexerParameters.ElementAt(context.IndexerParameters.Count - num);


                return Expression.Call(Visit(node.Object), node.Method, index); 
            }
        }
    }
}