using System.Collections.Generic;
using System.Linq;
using TwoWayMapper.Configuration;
using Xunit;

namespace TwoWayMapper.Test
{
    public class MapPathTest
    {
        private MapperMemberConfiguration<Container<ArrA>, int, Container<ArrB>, int> mapper;
        private Dictionary<string, string> leftToRightData;

        public MapPathTest()
        {
            mapper = new Mapper<Container<ArrA>, Container<ArrB>>()
                .Map(
                    (container, i, j) => container.Set[i].Values[j].Name,
                    (container, i, j) => container.Set[i].Data[j].Identifier)
                .Map(
                    (container, i, j) => container.Set[i].Values[j].Value,
                    (container, i, j) => container.Set[i].Data[j].Version)
                .Map(
                    (c, i) => c.Origin.Values[i].Name,
                    (c, i) => c.Origin.Data[i].Identifier)
                .Map(
                    c => c.Id,
                    c => c.Id);

            leftToRightData = new Dictionary<string, string>
            {
                { "Set[1].Values[2].Name",
                    "Set[1].Data[2].Identifier" },
                { "Set[5].Values[11].Value",
                    "Set[5].Data[11].Version" },
                { "Origin.Values[1345].Name",
                    "Origin.Data[1345].Identifier" },
                { "Id", "Id" }
            };
        }

        [Fact]
        public void MapPath_WhenInvoked_ShouldMapLeftToRightCorrectly()
        {
            // Arrange
            var src = leftToRightData.Keys;
            var expected = leftToRightData.Values.ToArray();

            // Act
            var actual = mapper.LeftPathToRightPath(src);

            // Assert
            Assert.Equal(expected, actual);
        }
        [Fact]
        public void MapPath_WhenInvoked_ShouldMapRightToLeftCorrectly()
        {
            // Arrange
            var src = leftToRightData.Values.ToArray();
            var expected = leftToRightData.Keys;

            // Act
            var actual = mapper.RightPathToLeftPath(src);

            // Assert
            Assert.Equal(expected, actual);
        }

        public class A
        {
            public string Name { get; set; }

            public string Value { get; set; }
        }

        public class B
        {
            public string Identifier { get; set; }

            public string Version { get; set; }
        }

        public class ArrA
        {
            public List<A> Values { get; set; }
        }

        public class ArrB
        {
            public B[] Data { get; set; }
        }

        public class Container<T>
        {
            public int Id { get; set; }

            public T Origin { get; set; }

            public T[] Set { get; set; }
        }
    }
}