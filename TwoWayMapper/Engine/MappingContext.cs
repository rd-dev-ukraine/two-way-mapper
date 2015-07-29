using System;
using System.Collections.Generic;
using TwoWayMapper.Conversion;

namespace TwoWayMapper.Engine
{
    public class MappingContext
    {
        public MappingContext(object[] converters)
        {
            if (converters == null)
                throw new ArgumentNullException(nameof(converters));

            Converters = converters;
            Errors = new List<ConversionError>();
        }

        public List<ConversionError> Errors { get; }

        public object[] Converters { get; }
    }
}