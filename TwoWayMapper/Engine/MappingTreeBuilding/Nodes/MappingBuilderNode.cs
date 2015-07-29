using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using TwoWayMapper.Engine.ExpressionParsing;

namespace TwoWayMapper.Engine.MappingTreeBuilding.Nodes
{
    public abstract class MappingBuilderNode
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        protected MappingBuilderNode()
        {
            Children = new List<MappingBuilderNode>();
        }

        public abstract string Path { get; }

        public List<MappingBuilderNode> Children { get; set; } 

        public abstract IEnumerable<Expression> BuildMapping(MappingBuilderContext context);

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        /// A string that represents the current object.
        /// </returns>
        public override string ToString()
        {
            return Path;
        }

        protected BlockExpression ChildMapping(MappingBuilderContext context) => Expression.Block(Children.SelectMany(ch => ch.BuildMapping(context)));

        protected Expression DebugOutput(string message, params Expression[] formattingParameters)
        {
            var writeLineMethod = typeof(Debug).GetMethod("WriteLine", new[] { typeof(string), typeof(object[]) });

            var parameters = new List<Expression>();
            if (formattingParameters != null)
                parameters.AddRange(formattingParameters);
            var arr = Expression.NewArrayInit(typeof(object), parameters.Select(p => Expression.Convert(p, typeof(object))));

            return Expression.Call(writeLineMethod, Expression.Constant(message), arr);
        }

        protected Expression BuildSourceNullChecksCondition(MappingBuilderContext context)
        {
            var common = Sources()
                .Select(src => new CommonPart
                {
                    Source = src,
                    SourceParts = ExpressionParser.SplitExpression(src)
                        .Where(a => !String.IsNullOrEmpty(a.Path))
                        .Where(a => !a.IsMostRightPart)
                        .Where(a => ExpressionHelper.GetIndexerCount(a.Member) <= context.IndexerParameters.Count)
                        .ToArray()
                })
                .ToArray();

            var commonMembers = new List<Expression>();
            var index = 0;

            while (true)
            {
                if (common.Any(a => a.SourceParts.Length <= index))
                    break;
                
                var parts = common.Select(c => c.SourceParts[index])
                                  .ToArray();
                if (parts.All(p => p.Path == parts[0].Path))
                {
                    commonMembers.Add(parts[0].Member);
                    if (parts[0].Indexer != null && 
                        ExpressionHelper.GetIndexerCount(parts[0].Indexer) <= context.IndexerParameters.Count)
                        commonMembers.Add(parts[0].Indexer);
                    index++;
                }
                else
                    break;
            }
            
            var condition = commonMembers.Aggregate<Expression, Expression>(
                Expression.Constant(true),
                (acc, expr) => Expression.AndAlso(acc, Expression.NotEqual(context.Rewrite(expr), Expression.Constant(null))));

            return condition;
        }

        private IEnumerable<Expression> Sources()
        {
            return FindSourceExpressionRecursive(this);
        }

        private IEnumerable<Expression> FindSourceExpressionRecursive(MappingBuilderNode node)
        {
            var an = node as AssigningNode;
            if (an != null)
                yield return an.Source;
            else
                foreach (var ch in node.Children)
                    foreach (var src in FindSourceExpressionRecursive(ch))
                        yield return src;
        }

        private class CommonPart
        {
            public Expression Source { get; set; }

            public PathPart[] SourceParts { get; set; }
        }
    }
}