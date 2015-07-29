using System;
using System.Collections.Generic;
using TwoWayMapper.Conversion.Converters;

namespace TwoWayMapper.Conversion
{
    public class ConverterCollection
    {
        private readonly Dictionary<Type, object> Converters = new Dictionary<Type, object>();

        public static readonly ConverterCollection Default = new ConverterCollection();

        static ConverterCollection()
        {
            Default.RegisterDefaultConverters();
        }

        public void RegisterConverter<TFrom, TTo>(IConverter<TFrom, TTo> converter)
        {
            if (converter == null)
                throw new ArgumentNullException(nameof(converter));

            Converters[typeof(IConverter<TFrom, TTo>)] = converter;
        }

        public IConverter<TFrom, TTo> Get<TFrom, TTo>()
        {
            object result;
            if (Converters.TryGetValue(typeof(IConverter<TFrom, TTo>), out result))
                return (IConverter<TFrom, TTo>)result;
            return null;
        }

        private void RegisterDefaultConverters()
        {
            RegisterGenericConverter<Boolean>();
            RegisterGenericConverter<Byte>();
            RegisterGenericConverter<Char>();
            RegisterGenericConverter<DateTime>();
            RegisterGenericConverter<DateTimeOffset>();
            RegisterGenericConverter<Decimal>();
            RegisterGenericConverter<Double>();
            RegisterGenericConverter<Guid>();
            RegisterGenericConverter<Int16>();
            RegisterGenericConverter<Int32>();
            RegisterGenericConverter<Int64>();
            RegisterGenericConverter<SByte>();
            RegisterGenericConverter<Single>();
            RegisterGenericConverter<TimeSpan>();
            RegisterGenericConverter<UInt16>();
            RegisterGenericConverter<UInt32>();
            RegisterGenericConverter<UInt64>();

        }

        private void RegisterGenericConverter<T>()
            where T : struct
        {
            RegisterConverter(TryParseConverter<T>.FromString);
            RegisterConverter(TryParseConverter<T>.ToString);
        }
    }
}