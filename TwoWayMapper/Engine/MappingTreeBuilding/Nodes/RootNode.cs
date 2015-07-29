using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace TwoWayMapper.Engine.MappingTreeBuilding.Nodes
{
    public class RootNode : MappingBuilderNode
    {
        public Expression Root { get; set; }

        public override string Path => "";

        public override IEnumerable<Expression> BuildMapping(MappingBuilderContext context)
        {
            return Children.SelectMany(child => child.BuildMapping(context));
        }
    }
}