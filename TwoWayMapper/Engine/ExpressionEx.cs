using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace TwoWayMapper.Engine
{
    public static class ExpressionEx
    {
        public static MethodCallExpression Format(Expression formattingString, IEnumerable<Expression> arguments)
        {
            if (formattingString == null)
                throw new ArgumentNullException(nameof(formattingString));
            if (arguments == null)
                throw new ArgumentNullException(nameof(arguments));

            var formatMethod = typeof (String).GetMethod(
                nameof(String.Format),
                new[]
                {
                    typeof(string),
                    typeof(object[])
                });

            return Expression.Call(
                formatMethod,
                formattingString,
                Expression.NewArrayInit(
                    typeof(object), 
                    arguments.Select(a => Expression.Convert(a, typeof(object)))));
        }


        public static Expression NewOrDefault(Type type)
        {
            if (type.Namespace == typeof(String).Namespace)
                return Expression.Default(type);
            return Expression.New(type);
        }
    }
}