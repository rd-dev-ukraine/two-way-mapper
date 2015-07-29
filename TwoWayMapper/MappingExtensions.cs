using System;
using System.Collections.Generic;

namespace TwoWayMapper
{
    public static class MappingExtensions
    {
        public static TRight Map<TLeft, TRight>(this IMapper<TLeft, TRight> mapper, TLeft source)
            where TLeft : class
            where TRight : class, new()
        {
            if (mapper == null)
                throw new ArgumentNullException(nameof(mapper));
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            var dest = new TRight();
            mapper.LeftToRight(source, dest);

            return dest;
        }

        public static TLeft Map<TLeft, TRight>(this IMapper<TLeft, TRight> mapper, TRight source)
            where TLeft : class, new()
            where TRight : class
        {
            if (mapper == null)
                throw new ArgumentNullException(nameof(mapper));
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            var dest = new TLeft();
            mapper.RightToLeft(source, dest);

            return dest;
        }

        public static List<TLeft> MapCollection<TLeft, TRight>(
            this IMapper<TLeft, TRight> mapper,
            IEnumerable<TRight> source)
            where TLeft : class
            where TRight : class
        {
            if (mapper == null)
                throw new ArgumentNullException(nameof(mapper));
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return mapper.RightToLeftCollection(source);
        }

        public static List<TRight> MapCollection<TLeft, TRight>(
           this IMapper<TLeft, TRight> mapper,
           IEnumerable<TLeft> source)
           where TLeft : class
           where TRight : class
        {
            if (mapper == null)
                throw new ArgumentNullException(nameof(mapper));
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return mapper.LeftToRightCollection(source);
        }
    }
}