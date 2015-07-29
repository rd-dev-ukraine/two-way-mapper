using FluentAssertions;
using Ploeh.AutoFixture;
using Xunit;

namespace TwoWayMapper.Test
{
    public class NestedClassesMapperTest
    {
        private readonly IFixture fixture = new Fixture();

        [Fact]
        public void Map_WhenInvokedForDifferentLevelOfNesting_ShouldMapCorrectly()
        {
            // Arrange
            var mapper = new Mapper<Container<B>, Container<AContainer>>()
                .Map((e, i) => e.Data[i].Identifier, (e, i) => e.Data[i].A.Id);

            var src = fixture.Create<Container<B>>();

            // Act
            var dst = mapper.Map(src);

            // Assert
            dst.Data.Should().NotBeNull();
            dst.Data.Length.Should().Be(src.Data.Length);

            for (var i = 0; i < dst.Data.Length; i++)
                dst.Data[i].A.Id.Should().Be(src.Data[i].Identifier);
        }

        public class A
        {
            public int Id { get; set; }
        }

        public class B
        {
            public int Identifier { get; set; }
        }

        public class AContainer
        {
            public A A { get; set; }
        }

        public class Container<T>
        {
            public T[] Data { get; set; }
        }
    }
}