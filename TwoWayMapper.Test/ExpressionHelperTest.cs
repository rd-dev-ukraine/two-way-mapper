using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using TwoWayMapper.Engine;
using Xunit;

namespace TwoWayMapper.Test
{
    public class ExpressionHelperTest
    {
        [Fact]
        public void GetIndexerCount_WhenInvokedForExprWithoutIndexer_ShouldReturnZero()
        {
            // Arrange
            Expression<Func<C, C>> expr = c => c;

            // Act
            var count = ExpressionHelper.GetIndexerCount(expr);

            // Assert
            Assert.Equal(count, 0);
        }

        [Fact]
        public void GetIndexerCount_WhenInvokedForExprWithArrayIndexer_ShouldReturnCorrectCount()
        {
            // Arrange
            Expression<Func<B, int, C>> expr = (b, i) => b.C[i];

            // Act
            var count = ExpressionHelper.GetIndexerCount(expr);

            // Assert
            Assert.Equal(count, 1);
        }

        [Fact]
        public void GetIndexerCount_WhenInvokedForExprWithListIndexer_ShouldReturnCorrectCount()
        {
            // Arrange
            Expression<Func<A, int, int, C>> expr = (x, i, j) => x.B[i].C[j];

            // Act
            var count = ExpressionHelper.GetIndexerCount(expr);

            // Assert
            Assert.Equal(count, 2);
        }

        public class A
        {
            public List<B> B { get; set; }
        }

        public class B
        {
            public C[] C { get; set; }
        }

        public class C
        { }
    }
}