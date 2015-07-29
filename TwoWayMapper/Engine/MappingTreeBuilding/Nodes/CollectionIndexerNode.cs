using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace TwoWayMapper.Engine.MappingTreeBuilding.Nodes
{
    public class CollectionIndexerNode : MappingBuilderNode
    {
        public MemberExpression Member { get; set; }

        public MethodCallExpression Indexer { get; set; }

        public override string Path => Indexer.ToPathWithEmptyIndexer();

        public override IEnumerable<Expression> BuildMapping(MappingBuilderContext context)
        {
            var index = context.CreateIteratorVariable();
            var source = FindChildSource(Children.First());
            var srcIndexer = ExpressionHelper.TrimIndexer(source, Indexer);
            var breakLabel = Expression.Label();

            var nullCheckCondition = BuildSourceNullChecksCondition(context);

            context.IndexerParameters.Push(index);

            var block =
                Expression.IfThen(
                    nullCheckCondition,
                    Expression.Block(
                        new[] { index },
                        Expression.Assign(index, Expression.Constant(0)),
                        Expression.Assign(context.Rewrite(Member), Expression.New(GetMemberType())),
                        Expression.Loop(
                            Expression.Block(
                                Expression.IfThenElse(
                                    Expression.LessThan(index, ExpressionHelper.GetLengthExpressionFromIndexer(context.Rewrite(srcIndexer))),
                                    Expression.IfThenElse(
                                        Expression.NotEqual(context.Rewrite(srcIndexer), Expression.Constant(null)),
                                        Expression.Block(
                                            Expression.Call(
                                                context.Rewrite(Member),
                                                "Add",
                                                new Type[] { },
                                                ExpressionEx.NewOrDefault(GetCollectionElementType())),
                                            ChildMapping(context)),
                                        Expression.Call(
                                            context.Rewrite(Member),
                                            "Add",
                                            new Type[] { },
                                            Expression.Default(GetCollectionElementType()))),
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

        private Type GetCollectionElementType()
        {
            var memberType = GetMemberType();
            var collectionType = memberType.GetInterfaces()
                    .Where(i => i.IsGenericType)
                    .Where(i => i.GetGenericTypeDefinition() == typeof(ICollection<>))
                    .Select(i => i.GetGenericArguments()[0])
                    .First();

            return collectionType;
        }

        private Type GetMemberType()
        {
            if (Member.Member.MemberType == MemberTypes.Property)
                return ((PropertyInfo)Member.Member).PropertyType;
            if (Member.Member.MemberType == MemberTypes.Field)
                return ((FieldInfo)Member.Member).FieldType;

            throw new ArgumentException("Only properties and fields are supported");
        }

    }
}