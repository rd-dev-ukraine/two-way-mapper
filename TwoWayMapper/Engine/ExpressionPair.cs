﻿using System.Linq.Expressions;
using TwoWayMapper.Conversion;

namespace TwoWayMapper.Engine
{
    public class ExpressionPair
    {
        public Expression Source { get; set; }
        
        public Expression Dest { get; set; }

        public object Converter { get; set; }
    }
}