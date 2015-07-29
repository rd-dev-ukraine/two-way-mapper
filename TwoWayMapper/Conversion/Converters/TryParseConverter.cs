using System;
using System.Linq.Expressions;

namespace TwoWayMapper.Conversion.Converters
{
    /// <summary>
    /// Convertes automatically generates conversion code from and to <typeparamref name="T"/>
    /// type using TryParse method
    /// </summary>
    public class TryParseConverter<T>
        where T : struct
    {
        private static readonly TryParseDelegate TryParse = BuildConversionMethod();
        public static readonly IConverter<string, T> FromString = new FromStringConverter();
        public static readonly new IConverter<T, string> ToString = new ToStringConverter();

        public static string ErrorMessage = "Value has invalid format";

        private static TryParseDelegate BuildConversionMethod()
        {
            var input = Expression.Parameter(typeof(string), "input");
            var output = Expression.Parameter(typeof(T).MakeByRefType(), "output");

            var returnTarget = Expression.Label(typeof(bool));
            var returnLabel = Expression.Label(returnTarget, Expression.Default(typeof(bool)));

            var result = Expression.Parameter(typeof(Boolean), "result");
            var lambda = Expression.Lambda<TryParseDelegate>(
                        Expression.Block(
                            typeof(bool),
                            new ParameterExpression[] { result },
                            new Expression[]
                            {
                                Expression.Assign(
                                    result,
                                    Expression.Call(
                                        typeof(T),
                                        "TryParse",
                                        Type.EmptyTypes,
                                        new Expression[] { input, output })),
                                Expression.Return(returnTarget, result),
                                returnLabel
                            }),
                        input,
                        output);

            return lambda.Compile();
        }

        private class FromStringConverter : IConverter<string, T>
        {
            public bool TryConvert(object source, out object destination, out string error)
            {
                destination = null;
                error = null;

                T result;

                if (TryParse((string)source, out result))
                {
                    destination = result;
                    return true;
                }

                error = ErrorMessage;

                return false;
            }

            public ConversionResult<string, T> Convert(string value)
            {
                T result;
                if (TryParse(value, out result))
                {
                    return ConversionResult<string, T>.Ok(result, value);
                }
                else
                {
                    return ConversionResult<string, T>.Fail(ErrorMessage, value);
                }
            }
        }

        private class ToStringConverter : IConverter<T, string>
        {
            public ConversionResult<T, string> Convert(T value)
            {
                return ConversionResult<T, string>.Ok(System.Convert.ToString(value), value);
            }
        }

        private delegate bool TryParseDelegate(string source, out T result);
    }
}