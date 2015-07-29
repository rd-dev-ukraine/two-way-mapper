using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using TwoWayMapper.Conversion;

namespace TwoWayMapper.Engine.MappingTreeBuilding.Nodes
{
    public class AssigningNode : MappingBuilderNode
    {
        public Expression Source { get; set; }

        public Expression Destination { get; set; }

        public object Converter { get; set; }

        public override string Path => Destination.ToPathWithEmptyIndexer();

        public override IEnumerable<Expression> BuildMapping(MappingBuilderContext context)
        {
            var sourceMember = context.Rewrite(Source);
            var destinationMember = context.Rewrite(Destination);
            var sourceMemberType = ExpressionHelper.GetMemberTypeOrCollectionElementType(Source);
            var destMemberType = ExpressionHelper.GetMemberTypeOrCollectionElementType(Destination);

            if (Converter != null)
            {
                var converterIndex = context.Converters.ToList().IndexOf(Converter);
                var sourcePropertyPathFormat = sourceMember.ToPathWithFormatting();
                var destPropertyPathFormat = destinationMember.ToPathWithFormatting();

                var converterType = typeof(IConverter<,>).MakeGenericType(sourceMemberType, destMemberType);
                var conversionResultType = typeof(ConversionResult<,>).MakeGenericType(sourceMemberType, destMemberType);

                var converterVar = Expression.Parameter(converterType, "converter");
                var conversionResult = Expression.Parameter(conversionResultType, "conversionResult");
                var destValueVar = Expression.Parameter(typeof(object), "destValue");


                yield return Expression.Block(
                    new ParameterExpression[] { converterVar, destValueVar, conversionResult },
                    Expression.Assign(
                        converterVar,
                        Expression.Convert(
                            Expression.ArrayIndex(
                                Expression.Property(context.ContextParameter, nameof(MappingContext.Converters)),
                                Expression.Constant(converterIndex)),
                            converterType)),
                    Expression.Assign(
                        conversionResult,
                        Expression.Call(
                            converterVar,
                            nameof(IConverter<object, object>.Convert),
                            new Type[] { },
                            sourceMember)),
                    Expression.IfThenElse(
                        Expression.Property(conversionResult, nameof(ConversionResult<object, object>.IsConvertedSuccessfully)),
                        MakeAssignment(
                            context,
                            destinationMember,
                            Expression.Property(conversionResult, nameof(ConversionResult<object, object>.ConvertedValue))),
                        AddConversionError(context, sourcePropertyPathFormat, destPropertyPathFormat, conversionResult, sourceMember)));

            }
            else
            {
                if (!destMemberType.IsAssignableFrom(sourceMemberType))
                    sourceMember = Expression.Convert(sourceMember, destMemberType);

                yield return MakeAssignment(context, destinationMember, sourceMember);
            }
        }

        private static BlockExpression AddConversionError(
            MappingBuilderContext context,
            string sourcePropertyPathFormat,
            string destPropertyPathFormat,
            ParameterExpression conversionResult,
            Expression sourceMember)
        {
            var conversionErrorInfoObjVar = Expression.Parameter(typeof(ConversionError), "conversionError");
            return Expression.Block(
                new[] { conversionErrorInfoObjVar },
                Expression.Assign(conversionErrorInfoObjVar, Expression.New(typeof(ConversionError))),
                Expression.Assign(
                    Expression.Property(conversionErrorInfoObjVar, nameof(ConversionError.SourcePath)),
                    Expression.Call(
                        typeof(AssigningNode),
                        nameof(AssigningNode.JoinPath),
                        null,
                        ExpressionEx.Format(
                            Expression.Constant(sourcePropertyPathFormat),
                            context.IndexerParameters.Reverse()),
                        Expression.Property(conversionResult, nameof(ConversionResult<int, int>.SourcePathSuffix)))),
                Expression.Assign(
                    Expression.Property(conversionErrorInfoObjVar, nameof(ConversionError.DestinationPath)),
                    ExpressionEx.Format(
                        Expression.Constant(destPropertyPathFormat),
                        context.IndexerParameters.Reverse())),
                Expression.Assign(
                    Expression.Property(conversionErrorInfoObjVar, nameof(ConversionError.ErrorMessage)),
                    Expression.Property(conversionResult, nameof(ConversionResult<int, int>.ErrorMessage))),
                Expression.Assign(
                    Expression.Property(conversionErrorInfoObjVar, nameof(ConversionError.SourceValue)),
                    Expression.Convert(Expression.Property(conversionResult, nameof(ConversionResult<int, int>.SourceValue)), typeof(object))),
                Expression.Call(
                    Expression.Property(context.ContextParameter, nameof(MappingContext.Errors)),
                    "Add",
                    Type.EmptyTypes,
                    conversionErrorInfoObjVar)
                );
        }

        private Expression MakeAssignment(MappingBuilderContext context, Expression destinationMember, Expression sourceMember)
        {
            if (destinationMember.NodeType == ExpressionType.ArrayIndex)
            {
                var binaryExpression = (BinaryExpression)destinationMember;
                return Expression.Assign(Expression.ArrayAccess(binaryExpression.Left, binaryExpression.Right), sourceMember);
            }
            else if (destinationMember.NodeType == ExpressionType.Call)
            {
                var callExpr = (MethodCallExpression)destinationMember;

                return
                    Expression.Assign(
                        Expression.MakeIndex(
                            callExpr.Object,
                            callExpr.Method.ReflectedType.GetProperty("Item"),
                            callExpr.Arguments),
                        sourceMember);
            }
            else
                return Expression.IfThen(
                    BuildSourceNullChecksCondition(context),
                    Expression.Assign(destinationMember, sourceMember));
        }

        public static string JoinPath(string path, string pathSuffix)
        {
            if (!String.IsNullOrWhiteSpace(pathSuffix))
            {
                if (pathSuffix.StartsWith("["))
                    return String.Concat(path, pathSuffix);
                else
                    return String.Concat(path, ".", pathSuffix);
            }
            return path;
        }
    }
}