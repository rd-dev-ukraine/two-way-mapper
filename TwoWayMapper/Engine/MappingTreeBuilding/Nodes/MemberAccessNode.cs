using System.Collections.Generic;
using System.Linq.Expressions;

namespace TwoWayMapper.Engine.MappingTreeBuilding.Nodes
{
    public class MemberAccessNode : MappingBuilderNode
    {
        public MemberExpression Member { get; set; }

        public override string Path => Member.ToPathWithEmptyIndexer();

        public override IEnumerable<Expression> BuildMapping(MappingBuilderContext context)
        {
            yield return Expression.Block(
                Expression.IfThen(
                    BuildSourceNullChecksCondition(context),
                    Expression.Block(
                         Expression.IfThen(
                            Expression.Equal(context.Rewrite(Member), Expression.Constant(null)),
                            Expression.Assign(context.Rewrite(Member), Expression.New(Member.Type))),
                            ChildMapping(context)
                         )));
        }
    }
}