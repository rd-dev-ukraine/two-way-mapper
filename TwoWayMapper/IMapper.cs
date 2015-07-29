using System.Collections.Generic;

namespace TwoWayMapper
{
    public interface IMapper<TLeft, TRight>
        where TLeft : class
        where TRight : class
    {
        void LeftToRight(TLeft source, TRight destination);

        void RightToLeft(TRight source, TLeft destination);

        string[] LeftPathToRightPath(IEnumerable<string> pathes);

        string[] RightPathToLeftPath(IEnumerable<string> pathes);

        List<TRight> LeftToRightCollection(IEnumerable<TLeft> source);

        List<TLeft> RightToLeftCollection(IEnumerable<TRight> source);
    }
}