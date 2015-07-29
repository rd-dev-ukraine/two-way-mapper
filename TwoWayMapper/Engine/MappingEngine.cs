using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

using TwoWayMapper.Conversion;
using TwoWayMapper.Engine.MappingTreeBuilding;

namespace TwoWayMapper.Engine
{
    /// <summary>
    /// How to map expression with indexers?
    /// 
    /// Basically, this is the same as mapping regular expressions, but needs additional info and steps
    /// At first, mapping expression with indexer finally looks like regular mapping, but needs additional info with indexes:
    /// 
    /// dest.Nested.Array[i].Name = source.NestedSource.ArraySource[i].Name;
    /// 
    /// This expression should be executed for all elements of source:
    /// 
    /// for(var i = 0; i &lt; source.NestedSource.ArraySource.Count(); i++)
    /// {
    ///    ... mapping here
    /// }
    /// 
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    /// <typeparam name="TDest"></typeparam>
    public class MappingEngine<TSource, TDest>
        where TSource : class
        where TDest : class
    {
        private readonly MappingEngineMemberMappingInfo[] mappingInfo;
        private readonly PathMappingInfo[] pathMappingInfo;
        private readonly object[] converters;

        private Action<TSource, TDest, MappingContext> mappingMethod;

        public MappingEngine(IEnumerable<MappingEngineMemberMappingInfo> mappingInfo)
        {
            this.mappingInfo = mappingInfo.ToArray();
            this.converters =  this.mappingInfo.Select(i => i.Converter)
                                         .Where(c => c != null)
                                         .Distinct()
                                         .ToArray();

            this.pathMappingInfo = mappingInfo
                .Select(info => new PathMappingInfo
                    {
                        SourcePathNoIndex = info.Source.ToPathWithEmptyIndexer(),
                        DestinationPathFormattingString = info.Dest.ToPathWithFormatting()
                    })
                .ToArray();
        }

        public void Map(TSource source, TDest dest)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (dest == null) throw new ArgumentNullException(nameof(dest));

            if (mappingMethod == null)
                mappingMethod = Build();

            var context = new MappingContext(converters);

            mappingMethod(source, dest, context);

            if (context.Errors.Any())
                throw new ConversionException(context.Errors);
        }

        public List<TDest> MapCollection(IEnumerable<TSource> source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (mappingMethod == null)
                mappingMethod = Build();

            var result = new List<TDest>();
            var errors = new List<ConversionError>();

            var index = 0;
            foreach (var src in source)
            {
                var context = new MappingContext(converters);
                var dest = Activator.CreateInstance<TDest>();

                var prefix = "[" + index + "].";

                mappingMethod(src, dest, context);
                result.Add(dest);

                if (context.Errors.Any())
                    errors.AddRange(
                        context.Errors.Select(e => new ConversionError
                        {
                            DestinationPath = prefix + e.DestinationPath,
                            SourceValue = e.SourceValue,
                            SourcePath = prefix + e.SourcePath,
                            ErrorMessage = e.ErrorMessage
                        }));

                index++;
            }

            if (errors.Any())
                throw new ConversionException(errors);

            return result;
        }

        public IEnumerable<string> MapPathes(IEnumerable<string> sourcePathes)
        {
            if (sourcePathes == null)
                throw new ArgumentNullException(nameof(sourcePathes));

            foreach (var src in sourcePathes)
            {
                var sourcePathNoIndex = ExpressionPathStringHelper.EmptyPathIndexer(src);
                var pathMapping = pathMappingInfo.FirstOrDefault(p => StringComparer.Ordinal.Equals(p.SourcePathNoIndex, sourcePathNoIndex));
                if (pathMapping != null)
                {
                    var indexes = ExpressionPathStringHelper.ExtractIndexes(src)
                        .Select(i => (object)i)
                        .ToArray();
                    yield return String.Format(
                        pathMapping.DestinationPathFormattingString,
                        indexes);
                }
                else
                    yield return src;
            }
        }

        private Action<TSource, TDest, MappingContext> Build()
        {
            var sourceParam = Expression.Parameter(typeof(TSource), "source");
            var destParam = Expression.Parameter(typeof(TDest), "dest");

            var mappings = new List<ExpressionPair>();

            foreach (var mapping in mappingInfo)
            {
                var sourceLambda = (LambdaExpression)mapping.Source;
                var destLambda = (LambdaExpression)mapping.Dest;

                var sourceMember = sourceLambda.Body;
                
                var destMember = destLambda.Body;
                
                sourceMember = ExpressionHelper.RewriteMemberSource(sourceMember, sourceLambda.Parameters[0], sourceParam);
                destMember = ExpressionHelper.RewriteMemberSource(destMember, destLambda.Parameters[0], destParam);

                mappings.Add(new ExpressionPair
                {
                    Source = sourceMember,
                    Dest = destMember,
                    Converter = mapping.Converter
                });
            }

            var nodes = ExpressionTreeBuilder.BuildExpressionTree(mappings.ToArray());
            var context = new MappingBuilderContext(converters);
            var block = Expression.Block(nodes.SelectMany(n => n.BuildMapping(context)));

            var lambda = Expression.Lambda<Action<TSource, TDest, MappingContext>>(block, sourceParam, destParam, context.ContextParameter);
            return lambda.Compile();
        }

        private class PathMappingInfo
        {
            public string SourcePathNoIndex { get; set; }

            public string DestinationPathFormattingString { get; set; }
        }
    }
}