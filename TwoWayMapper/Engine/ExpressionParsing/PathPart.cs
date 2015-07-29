using System.Linq.Expressions;

namespace TwoWayMapper.Engine.ExpressionParsing
{
    public class PathPart
    {
        public Expression Member { get; set; } 

        public Expression Indexer { get; set; }

        public string Path { get; set; }

        public bool IsMostRightPart { get; set; }

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
    }
}