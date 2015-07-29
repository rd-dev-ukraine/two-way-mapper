using TwoWayMapper.Engine;
using Xunit;

namespace TwoWayMapper.Test
{
    public class ExpressionPathStringHelperTest
    {
        [Fact]
        public void EmptyPathIndexer_WhenInvoked_ShouldEmptyIndexes()
        {
            // Arrange
            var path = "[2].Name.Val[1].Value";

            // Act
            var result = ExpressionPathStringHelper.EmptyPathIndexer(path);

            // Assert
            Assert.Equal("[].Name.Val[].Value", result);
        }

        [Fact]
        public void EmptyPathIndexer_WhenInvokedForNoIndexer_ShouldKeepString()
        {
            // Arrange
            var path = "Name.Value";

            // Act
            var result = ExpressionPathStringHelper.EmptyPathIndexer(path);

            // Assert
            Assert.Equal(path, result);
        }
    }
}