using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace TwoWayMapper.Engine.ExpressionParsing
{
    public static class ExpressionParser
    {
        public static IEnumerable<PathPart> SplitExpression(Expression expression)
        {
            if (expression == null) throw new ArgumentNullException(nameof(expression));

            return SplitExpressionRecursive(expression, true);
        }

        private static IEnumerable<PathPart> SplitExpressionRecursive(Expression expression, bool isRootCall)
        {
            var node = expression;
            Expression index = null;

            if (expression.NodeType == ExpressionType.Index)
            {
                var idx = (IndexExpression) expression;
                index = idx;
                node = idx.Object;
            }

            if (expression.NodeType == ExpressionType.Call)
            {
                var idx = (MethodCallExpression) expression;
                index = idx;
                node = idx.Object;
            }

            if (expression.NodeType == ExpressionType.ArrayIndex)
            {
                var idx = (BinaryExpression) expression;
                index = idx;
                node = idx.Left;
            }

            var pathPart = new PathPart
            {
                Indexer = index,
                Member = node,
                Path = expression.ToPathWithEmptyIndexer(),
                IsMostRightPart = isRootCall
            };

            if (node.NodeType == ExpressionType.MemberAccess)
            {
                var memberAccess = (MemberExpression) node;
                foreach (var parent in SplitExpressionRecursive(memberAccess.Expression, false))
                    yield return parent;
            }

            yield return pathPart;
        }
    }
}