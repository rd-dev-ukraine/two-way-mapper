using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using TwoWayMapper.Conversion;

namespace TwoWayMapper.Configuration
{
    public class MapperMemberConfiguration<TLeft, TLeftVal, TRight, TRightVal> : IMapper<TLeft, TRight>
        where TLeft : class
        where TRight : class
    {
        private readonly Mapper<TLeft, TRight> mapper;
        private readonly MapperMember member;

        public MapperMemberConfiguration(Mapper<TLeft, TRight> mapper, MapperMember member)
        {
            if (mapper == null)
                throw new ArgumentNullException(nameof(mapper));
            if (member == null)
                throw new ArgumentNullException(nameof(member));

            this.mapper = mapper;
            this.member = member;

            this.member.LeftToRightConverter = mapper.DefaultConverters.Get<TLeftVal, TRightVal>();
            this.member.RightToLeftConverter = mapper.DefaultConverters.Get<TRightVal, TLeftVal>();
        }

        public MapperMemberConfiguration<TLeft, TLeftValue, TRight, TRightValue> Map<TLeftValue, TRightValue>(
            Expression<Func<TLeft, TLeftValue>> leftExpr,
            Expression<Func<TRight, TRightValue>> rightExpr) => mapper.Map(leftExpr, rightExpr);

        public MapperMemberConfiguration<TLeft, TLeftValue, TRight, TRightValue> Map<TLeftValue, TRightValue>(
            Expression<Func<TLeft, int, TLeftValue>> leftExpr,
            Expression<Func<TRight, int, TRightValue>> rightExpr) => mapper.Map(leftExpr, rightExpr);

        public MapperMemberConfiguration<TLeft, TLeftValue, TRight, TRightValue> Map<TLeftValue, TRightValue>(
            Expression<Func<TLeft, int, int, TLeftValue>> leftExpr,
            Expression<Func<TRight, int, int, TRightValue>> rightExpr) => mapper.Map(leftExpr, rightExpr);

        public MapperMemberConfiguration<TLeft, TLeftVal, TRight, TRightVal> Convert(
            IConverter<TLeftVal, TRightVal> leftToRight, IConverter<TRightVal, TLeftVal> rightToLeft)
        {
            member.LeftToRightConverter = leftToRight;
            member.RightToLeftConverter = rightToLeft;

            return this;
        }

        public MapperMemberConfiguration<TLeft, TLeftVal, TRight, TRightVal> Convert<TLeftToRightConverter, TRightToLeftConverter>()
            where TLeftToRightConverter : IConverter<TLeftVal, TRightVal>, new()
            where TRightToLeftConverter : IConverter<TRightVal, TLeftVal>, new() => Convert(new TLeftToRightConverter(), new TRightToLeftConverter());

        public string[] LeftPathToRightPath(IEnumerable<string> pathes) => mapper.LeftPathToRightPath(pathes);

        public string[] RightPathToLeftPath(IEnumerable<string> pathes) => mapper.RightPathToLeftPath(pathes);

        public List<TRight> LeftToRightCollection(IEnumerable<TLeft> source) => mapper.LeftToRightCollection(source);

        public List<TLeft> RightToLeftCollection(IEnumerable<TRight> source) => mapper.RightToLeftCollection(source);

        public void LeftToRight(TLeft source, TRight destination) => mapper.LeftToRight(source, destination);

        public void RightToLeft(TRight source, TLeft destination) => mapper.RightToLeft(source, destination);
    }
}