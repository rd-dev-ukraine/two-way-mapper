using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using TwoWayMapper.Configuration;
using TwoWayMapper.Conversion;
using TwoWayMapper.Engine;

namespace TwoWayMapper
{
    public class Mapper<TLeft, TRight> : IMapper<TLeft, TRight>
        where TLeft : class
        where TRight : class
    {
        private readonly List<MapperMember> expressions = new List<MapperMember>();

        private MappingEngine<TLeft, TRight> leftToRight;
        private MappingEngine<TRight, TLeft> rightToLeft;
        
        public ConverterCollection DefaultConverters => ConverterCollection.Default;

        public MapperMemberConfiguration<TLeft, TLeftValue, TRight, TRightValue> Map<TLeftValue, TRightValue>(
            Expression<Func<TLeft, TLeftValue>> leftExpr,
            Expression<Func<TRight, TRightValue>> rightExpr)
        {
            if (leftExpr == null)
                throw new ArgumentNullException(nameof(leftExpr));
            if (rightExpr == null)
                throw new ArgumentNullException(nameof(rightExpr));

            var member = AddMappingExpression(leftExpr, rightExpr);

            return new MapperMemberConfiguration<TLeft, TLeftValue, TRight, TRightValue>(this, member);
        }

        public MapperMemberConfiguration<TLeft, TLeftValue, TRight, TRightValue> Map<TLeftValue, TRightValue>(
            Expression<Func<TLeft, int, TLeftValue>> leftExpr,
            Expression<Func<TRight, int, TRightValue>> rightExpr)
        {
            if (leftExpr == null) throw new ArgumentNullException(nameof(leftExpr));
            if (rightExpr == null) throw new ArgumentNullException(nameof(rightExpr));

            var member = AddMappingExpression(leftExpr, rightExpr);

            return new MapperMemberConfiguration<TLeft, TLeftValue, TRight, TRightValue>(this, member);
        }

        public MapperMemberConfiguration<TLeft, TLeftValue, TRight, TRightValue> Map<TLeftValue, TRightValue>(
            Expression<Func<TLeft, int, int, TLeftValue>> leftExpr,
            Expression<Func<TRight, int, int, TRightValue>> rightExpr)
        {
            if (leftExpr == null) throw new ArgumentNullException(nameof(leftExpr));
            if (rightExpr == null) throw new ArgumentNullException(nameof(rightExpr));

            var member = AddMappingExpression(leftExpr, rightExpr);

            return new MapperMemberConfiguration<TLeft, TLeftValue, TRight, TRightValue>(this, member);
        }

        public void LeftToRight(TLeft source, TRight destination)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (destination == null)
                throw new ArgumentNullException(nameof(destination));

            var engine = GetLeftToRightMappingEngine();
            engine.Map(source, destination);
        }

        public void RightToLeft(TRight source, TLeft destination)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (destination == null)
                throw new ArgumentNullException(nameof(destination));
            var engine = GetRightToLeftMappingEngine();
            engine.Map(source, destination);
        }

        public string[] LeftPathToRightPath(IEnumerable<string> pathes)
        {
            if (pathes == null)
                throw new ArgumentNullException(nameof(pathes));

            var engine = GetLeftToRightMappingEngine();
            var result = engine.MapPathes(pathes).ToArray();
            return result;
        }

        public string[] RightPathToLeftPath(IEnumerable<string> pathes)
        {
            if (pathes == null)
                throw new ArgumentNullException(nameof(pathes));

            var engine = GetRightToLeftMappingEngine();
            return engine.MapPathes(pathes).ToArray();
        }

        public List<TRight> LeftToRightCollection(IEnumerable<TLeft> source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            var engine = GetLeftToRightMappingEngine();

            return engine.MapCollection(source);
        }

        public List<TLeft> RightToLeftCollection(IEnumerable<TRight> source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            var engine = GetRightToLeftMappingEngine();

            return engine.MapCollection(source);
        }

        private MapperMember AddMappingExpression(Expression left, Expression right)
        {
            if (left == null)
                throw new ArgumentNullException(nameof(left));
            if (right == null)
                throw new ArgumentNullException(nameof(right));

            var mapperMember = new MapperMember
            {
                Left = left,
                Right = right
            };

            expressions.Add(mapperMember);

            return mapperMember;
        }

        private MappingEngine<TLeft, TRight> GetLeftToRightMappingEngine()
        {
            return leftToRight ??
                   (leftToRight =
                       new MappingEngine<TLeft, TRight>(
                           expressions.Select(
                               a => new MappingEngineMemberMappingInfo
                               {
                                   Source = a.Left,
                                   Dest = a.Right,
                                   Converter = a.LeftToRightConverter
                               })));
        }

        private MappingEngine<TRight, TLeft> GetRightToLeftMappingEngine()
        {
            return rightToLeft ??
                   (rightToLeft =
                       new MappingEngine<TRight, TLeft>(
                           expressions.Select(
                               a => new MappingEngineMemberMappingInfo
                               {
                                   Source = a.Right,
                                   Dest = a.Left,
                                   Converter = a.RightToLeftConverter
                               })));
        }
    }
}