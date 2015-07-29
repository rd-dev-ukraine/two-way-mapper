using System.Linq.Expressions;
using TwoWayMapper.Conversion;

namespace TwoWayMapper.Configuration
{
    public class MapperMember
    {
        public Expression Left { get; set; }

        public Expression Right { get; set; }

        public object LeftToRightConverter { get; set; }

        public object RightToLeftConverter { get; set; }
    }
}