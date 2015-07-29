using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace TwoWayMapper.Engine.MappingTreeBuilding.Nodes
{
    public class ArrayIndexerNode : MappingBuilderNode
    {
        public MemberExpression Member { get; set; }

        public BinaryExpression Indexer { get; set; }

        public override string Path => Indexer.ToPathWithEmptyIndexer();

        public override IEnumerable<Expression> BuildMapping(MappingBuilderContext context)
        {
            var index = context.CreateIteratorVariable();
            var source = FindChildSource(Children.First());
            var srcIndexer = ExpressionHelper.TrimIndexer(source, Indexer);
            var breakLabel = Expression.Label();

            var nullCheckCondition = BuildSourceNullChecksCondition(context);

            context.IndexerParameters.Push(index);

            var srcElementType = ExpressionHelper.GetMemberTypeOrCollectionElementType(srcIndexer);

            var block =
                Expression.IfThen(
                    nullCheckCondition,
                    Expression.Block(
                    new[] { index },
                    Expression.Assign(index, Expression.Constant(0)),
                    Expression.Assign(context.Rewrite(Member), BuildArrayInitializer(context.Rewrite(srcIndexer))),
                    Expression.Loop(
                        Expression.Block(
                            Expression.IfThenElse(
                                Expression.LessThan(index, ExpressionHelper.GetLengthExpressionFromIndexer(context.Rewrite(Indexer))),
                                Expression.IfThen(
                                    srcElementType.IsValueType 
                                        ? (Expression)Expression.Constant(true) 
                                        : Expression.NotEqual(context.Rewrite(srcIndexer), Expression.Constant(null)),
                                    Expression.Block(
                                        Expression.Assign(
                                            Expression.ArrayAccess(context.Rewrite(Member), index),
                                            ExpressionEx.NewOrDefault(GetArrayElementType())),
                                        ChildMapping(context))),
                            Expression.Break(breakLabel)),
                            Expression.Assign(index, Expression.Add(index, Expression.Constant(1)))),
                        breakLabel)));

            context.IndexerParameters.Pop();

            return new[] { block };
        }

        private Expression FindChildSource(MappingBuilderNode child)
        {
            var node = child as AssigningNode;
            if (node != null)
                return node.Source;

            return FindChildSource(child.Children.First());
        }

        private Expression BuildArrayInitializer(Expression sourceIndexer)
        {
            var length = ExpressionHelper.GetLengthExpressionFromIndexer(sourceIndexer);
            return Expression.NewArrayBounds(GetArrayElementType(), length);
        }

        private Type GetArrayElementType()
        {
            var memberType = ExpressionHelper.GetMemberType(Member);
            return memberType.GetElementType();
        }
    }
}