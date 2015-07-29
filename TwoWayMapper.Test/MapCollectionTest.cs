using System.Collections.Generic;
using System.Linq;
using Ploeh.AutoFixture;
using TwoWayMapper.Conversion;
using Xunit;

namespace TwoWayMapper.Test
{
    public class MapCollectionTest
    {
        private readonly IFixture fixture = new Fixture();
        private readonly IMapper<AContainer, BContainer> mapper;

        public MapCollectionTest()
        {
            mapper = new Mapper<AContainer, BContainer>()
                .Map((e, i) => e.Data[i].Name, (container, i) => container.Set[i].DisplayName);
        }

        [Fact]
        public void MapCollection_WhenInvoked_ShouldMapRightToLeft()
        {
            // Arrange
            var source = fixture.CreateMany<BContainer>().ToArray();

            // Act
            var target = mapper.MapCollection(source);

            // Assert
            Assert.NotNull(target);
            Assert.Equal(source.Length, target.Count);

            for (var i = 0; i < target.Count; i++)
            {
                var src = source[i];
                var dst = target[i];

                Assert.NotNull(src);
                Assert.NotNull(dst);

                Assert.NotNull(dst.Data);
                Assert.Equal(dst.Data.Count, src.Set.Count);

                for (var j = 0; j < dst.Data.Count; j++)
                {
                    Assert.Equal(src.Set[j].DisplayName, dst.Data[j].Name);
                }
            }
        }

        [Fact]
        public void MapCollection_WhenInvoked_ShouldMapLeftRight()
        {
            // Arrange
            var source = fixture.CreateMany<AContainer>().ToArray();

            // Act
            var target = mapper.MapCollection(source);

            // Assert
            Assert.NotNull(target);
            Assert.Equal(source.Length, target.Count);

            for (var i = 0; i < target.Count; i++)
            {
                var src = source[i];
                var dst = target[i];

                Assert.NotNull(src);
                Assert.NotNull(dst);

                Assert.NotNull(dst.Set);
                Assert.Equal(dst.Set.Count, src.Data.Count);

                for (var j = 0; j < dst.Set.Count; j++)
                {
                    Assert.Equal(src.Data[j].Name, dst.Set[j].DisplayName);
                }
            }
        }

        [Fact]
        public void MapCollection_WhenConversionFailed_ShouldCorrectlyAddIndexToPathes()
        {
            // Arrange
            var mapper = new Mapper<AContainer, BContainer>()
                .Map((e, i) => e.Data[i].Name, (container, i) => container.Set[i].Numeric);

            var left = fixture.CreateMany<AContainer>(15).ToList();

            left.ForEach(e => e.Data.ForEach(d => d.Name = "124"));

            left[11].Data[2] = new A { Name = "Error" };

            // Throws
            try
            {
                mapper.MapCollection(left);
                Assert.True(false);
            }
            catch (ConversionException ex)
            {
                Assert.Equal(1, ex.Errors.Length);
                var error = ex.Errors[0];

                Assert.Equal("[11].Data[2].Name", error.SourcePath);
                Assert.Equal("[11].Set[2].Numeric", error.DestinationPath);
                Assert.NotEmpty(error.ErrorMessage);
                Assert.Equal("Error", error.SourceValue);
            }
        }

        public class A
        {
            public string Name { get; set; }
        }

        public class B
        {
            public string DisplayName { get; set; }

            public int Numeric { get; set; }
        }

        public class AContainer
        {
            public List<A> Data { get; set; }
        }

        public class BContainer
        {
            public List<B> Set { get; set; } 
        }
    }
}