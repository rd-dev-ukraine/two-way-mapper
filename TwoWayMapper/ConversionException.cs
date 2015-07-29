using System;
using System.Collections.Generic;
using System.Linq;

namespace TwoWayMapper
{
    [Serializable]
    public class ConversionException : Exception
    {
        public ConversionException(IEnumerable<ConversionError> errors)
            : base("One or more conversion errors occured due mapping.")
        {
            if (errors == null)
                throw new ArgumentNullException(nameof(errors));

            Errors = errors.ToArray();
        }

        public ConversionError[] Errors { get; }
    }
}