using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using TwoWayMapper.Engine.ExpressionParsing;
using TwoWayMapper.Engine.MappingTreeBuilding.Nodes;

namespace TwoWayMapper.Engine.MappingTreeBuilding
{
    public static class ExpressionTreeBuilder
    {
        public static List<MappingBuilderNode> BuildExpressionTree(ExpressionPair[] mappings)
        {
            var expressionParts = mappings.Select(m => new ExpressionPart
            {
                Source = m.Source,
                SourcePath = m.Source.ToPathWithEmptyIndexer(),
                Parts = ExpressionParser.SplitExpression(m.Dest).ToArray(),
                Converter = m.Converter
            })
            .ToArray();

            var nodeMap = new Dictionary<string, MappingBuilderNode>();

            foreach (var part in expressionParts)
                CreateNodes(part.Parts, part.Source, part.Converter, 0, nodeMap);

            var result = nodeMap.Values.OfType<RootNode>()
                                       .Cast<MappingBuilderNode>()
                                       .ToList();

            return result;
        }

        private static void CreateNodes(
            PathPart[] parts,
            Expression source,
            object converter,
            int index,
            Dictionary<string, MappingBuilderNode> nodeMap,
            MappingBuilderNode parentNode = null)
        {
            var part = parts[index];
            MappingBuilderNode currentNode;

            if (!nodeMap.TryGetValue(part.Path, out currentNode))
            {
                // Create node
                var currentNodes = CreateNode(
                    part.Member,
                    part.Indexer,
                    index == parts.Length - 1 ? source : null,
                    converter).ToArray();

                for (var i = 0; i < currentNodes.Length - 1; i++)
                    currentNodes[i].Children.Add(currentNodes[i + 1]);

                foreach (var c in currentNodes)
                    if (!nodeMap.ContainsKey(c.Path))
                        nodeMap.Add(c.Path, c);


                parentNode?.Children.Add(currentNodes.First());

                currentNode = currentNodes.Last();
            }

            if (index + 1 < parts.Length)
                CreateNodes(parts, source, converter, index + 1, nodeMap, currentNode);
        }

        private static IEnumerable<MappingBuilderNode> CreateNode(
            Expression member,
            Expression indexer,
            Expression source,
            object converter)
        {

            if (indexer != null)
            {
                switch (indexer.NodeType)
                {
                    case ExpressionType.ArrayIndex:
                        yield return new ArrayIndexerNode
                        {
                            Indexer = (BinaryExpression)indexer,
                            Member = (MemberExpression)member
                        };
                        break;
                    case ExpressionType.Index:
                    case ExpressionType.Call:
                        yield return new CollectionIndexerNode
                        {
                            Indexer = (MethodCallExpression)indexer,
                            Member = (MemberExpression)member
                        };
                        break;
                }

                if (source != null)
                    yield return new AssigningNode
                    {
                        Source = source,
                        Destination = indexer,
                        Converter = converter
                    };

                yield break;
            }

            if (source != null)
            {
                yield return new AssigningNode
                {
                    Source = source,
                    Destination = member,
                    Converter = converter
                };

                yield break;
            }

            if (member.NodeType == ExpressionType.MemberAccess)
            {
                yield return new MemberAccessNode
                {
                    Member = (MemberExpression)member
                };
                yield break;
            }

            if (member.NodeType == ExpressionType.Parameter)
            {
                yield return new RootNode
                {
                    Root = member
                };
                yield break;
            }

            throw new ArgumentException($"Expression is not supported: {member.ToPathWithEmptyIndexer()}", nameof(member));
        }

        private class ExpressionPart
        {
            public PathPart[] Parts { get; set; }

            public Expression Source { get; set; }

            public string SourcePath { get; set; }

            public object Converter { get; set; }
        }
    }
}
