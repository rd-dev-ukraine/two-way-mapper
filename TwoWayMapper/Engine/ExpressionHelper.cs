using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace TwoWayMapper.Engine
{
    public static class ExpressionHelper
    {
        public static MemberInfo Member<T, TVal>(Expression<Func<T, TVal>> expr)
        {
            if (expr == null)
                throw new ArgumentNullException(nameof(expr));

            var lambda = (LambdaExpression)expr;
            if (lambda.Body.NodeType != ExpressionType.MemberAccess)
                throw new ArgumentException("Expression is not a member access", nameof(expr));

            return ((MemberExpression)lambda.Body).Member;
        }

        public static string ToPathWithEmptyIndexer(this Expression expression)
        {
            if (expression == null)
                throw new ArgumentNullException(nameof(expression));

            if (expression.NodeType == ExpressionType.Lambda)
            {
                var lambda = (LambdaExpression)expression;
                return ToPathWithEmptyIndexer(lambda.Body);
            }

            if (expression.NodeType == ExpressionType.Index)
            {
                var index = (IndexExpression)expression;
                var leftPart = ToPathWithEmptyIndexer(index.Object);
                return leftPart + "[]";
            }

            if (expression.NodeType == ExpressionType.Call)
            {
                var call = (MethodCallExpression)expression;
                var leftPart = ToPathWithEmptyIndexer(call.Object);
                if (String.IsNullOrWhiteSpace(leftPart))
                    return call.Method.Name;
                return $"{leftPart}[]";
            }

            if (expression.NodeType == ExpressionType.ArrayIndex)
            {
                var index = (BinaryExpression)expression;
                var leftPart = ToPathWithEmptyIndexer(index.Left);
                return leftPart + "[]";
            }

            if (expression.NodeType == ExpressionType.MemberAccess)
            {
                var memberExpression = (MemberExpression)expression;
                var memberName = memberExpression.Member.Name;
                var leftPart = ToPathWithEmptyIndexer(memberExpression.Expression);
                if (String.IsNullOrWhiteSpace(leftPart))
                    return memberName;
                return $"{leftPart}.{memberName}";
            }

            return String.Empty;
        }

        public static string ToPathWithFormatting
            (this Expression expression)
        {
            if (expression == null)
                throw new ArgumentNullException(nameof(expression));

            var pathWithEmptyIndex = expression.ToPathWithEmptyIndexer();

            var builder = new StringBuilder(pathWithEmptyIndex.Length + 40);

            var index = 0;
            foreach (var ch in pathWithEmptyIndex)
            {
                builder.Append(ch);
                if (ch == '[')
                {
                    builder.Append("{" + index + "}");
                    index++;
                }
            }

            return builder.ToString();
        }

        public static MemberExpression FindMemberExpression(Expression expression)
        {
            if (expression == null)
                throw new ArgumentNullException(nameof(expression));

            if (expression.NodeType == ExpressionType.MemberAccess)
                return (MemberExpression)expression;

            if (expression.NodeType == ExpressionType.Lambda)
                return FindMemberExpression(((LambdaExpression)expression).Body);

            return null;
        }

        public static Expression RewriteMemberSource(Expression sourceExpression, ParameterExpression sourceParameter, ParameterExpression targetParameter)
        {
            if (sourceExpression == null)
                throw new ArgumentNullException(nameof(sourceExpression));
            if (sourceParameter == null)
                throw new ArgumentNullException(nameof(sourceParameter));
            if (targetParameter == null)
                throw new ArgumentNullException(nameof(targetParameter));

            var replacer = new ParameterReplacerVisitor(sourceParameter, targetParameter);
            return replacer.Visit(sourceExpression);
        }

        /// <summary>
        /// Gets a new expression which is sub-expression of <paramref name="expressionToTrim"/> which corresponds to indexer passed to <paramref name="patternIndexer"/>.
        /// </summary>
        public static Expression TrimIndexer(Expression expressionToTrim, Expression patternIndexer)
        {
            if (expressionToTrim == null) throw new ArgumentNullException(nameof(expressionToTrim));
            if (patternIndexer == null) throw new ArgumentNullException(nameof(patternIndexer));

            var indexerNumber = GetIndexerCount(patternIndexer);

            var finder = new SubIndexerFinderVisitor(indexerNumber);

            finder.Visit(expressionToTrim);

            return finder.ResultingIndexer;
        }

        public static Expression GetLengthExpressionFromIndexer(Expression indexer)
        {
            if (indexer == null) throw new ArgumentNullException(nameof(indexer));

            if (indexer.NodeType == ExpressionType.ArrayIndex)
            {
                var node = (BinaryExpression)indexer;
                return Expression.ArrayLength(node.Left);
            }

            if (indexer.NodeType == ExpressionType.Index)
            {
                var node = (IndexExpression)indexer;
                return Expression.Property(node.Object, "Count");
            }

            if (indexer.NodeType == ExpressionType.Call)
            {
                var node = (MethodCallExpression)indexer;
                return Expression.Property(node.Object, "Count");
            }

            throw new ArgumentException("Can't get length", nameof(indexer));
        }

        public static int GetIndexerCount(Expression indexer)
        {
            if (indexer == null) throw new ArgumentNullException(nameof(indexer));

            var visitor = new IndexerCounterVisitor();

            visitor.Visit(indexer);

            return visitor.IndexerCount;
        }

        public static Type GetMemberType(MemberExpression member)
        {
            if (member.Member.MemberType == MemberTypes.Property)
                return ((PropertyInfo)member.Member).PropertyType;
            if (member.Member.MemberType == MemberTypes.Field)
                return ((FieldInfo)member.Member).FieldType;

            throw new ArgumentException("Only properties and fields are supported");
        }

        public static Type GetMemberTypeOrCollectionElementType(Expression expression)
        {
            if (expression == null)
                throw new ArgumentNullException(nameof(expression));

            if (expression.NodeType == ExpressionType.MemberAccess)
                return GetMemberType((MemberExpression)expression);

            if (expression.NodeType == ExpressionType.ArrayIndex)
            {
                var index = (BinaryExpression)expression;
                var memberAccess = (MemberExpression)index.Left;
                return GetMemberType(memberAccess).GetElementType();
            }

            if (expression.NodeType == ExpressionType.Call)
            {
                var call = (MethodCallExpression)expression;
                return call.Method.ReturnType;
            }

            throw new ArgumentException(
                $"Can't get member or element type from expression {expression.ToPathWithEmptyIndexer()}",
                nameof(expression));
        }

        private class ParameterReplacerVisitor : ExpressionVisitor
        {
            private readonly ParameterExpression source;
            private readonly ParameterExpression target;

            public ParameterReplacerVisitor(ParameterExpression source, ParameterExpression target)
            {
                this.source = source;
                this.target = target;
            }

            protected override Expression VisitParameter(ParameterExpression node)
            {
                // Replace the source with the target, visit other params as usual.
                return node == source ? target : base.VisitParameter(node);
            }
        }

        private class IndexerCounterVisitor : ExpressionVisitor
        {
            public int IndexerCount { get; private set; }

            protected override Expression VisitIndex(IndexExpression node)
            {
                IndexerCount = IndexerCount + 1;
                return base.VisitIndex(node);
            }

            protected override Expression VisitBinary(BinaryExpression node)
            {
                if (node.NodeType == ExpressionType.ArrayIndex)
                    IndexerCount = IndexerCount + 1;
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
                IndexerCount = IndexerCount + 1;
                return base.VisitMethodCall(node);
            }
        }

        private class SubIndexerFinderVisitor : ExpressionVisitor
        {
            private readonly int indexerNumberToFind;
            public SubIndexerFinderVisitor(int indexerNumberToFind)
            {
                this.indexerNumberToFind = indexerNumberToFind;
            }

            public Expression ResultingIndexer { get; private set; }

            public override Expression Visit(Expression node)
            {
                if (node.NodeType == ExpressionType.Index ||
                    node.NodeType == ExpressionType.ArrayIndex ||
                    node.NodeType == ExpressionType.Call)
                {
                    if (GetIndexerCount(node) == indexerNumberToFind)
                        ResultingIndexer = node;
                }
                return base.Visit(node);
            }
        }
    }
}