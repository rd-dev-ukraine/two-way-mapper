using FluentAssertions;

using Ploeh.AutoFixture;
using Xunit;

namespace TwoWayMapper.Test
{
    public class MapNullNestedObjectsTest
    {
        private readonly IFixture fixture = new Fixture(); 

        [Fact]
        public void Map_WhenInvokedForNestedObject_MustCreateHierarchy()
        {
            // Arrange
            var mapper = new Mapper<Model, AggregationRoot>()
                .Map(e => e.PageData.Model.Display, e => e.Container.Entity.Name);

            var src = fixture.Create<AggregationRoot>();

            // Act
            var dst = mapper.Map(src);

            // Assert
            dst.PageData.Should().NotBeNull();
            dst.PageData.Model.Should().NotBeNull();
            dst.PageData.Model.Display.Should().Be(src.Container.Entity.Name);
        }

        [Fact]
        public void Map_WhenInvokedForNullNestedObject_MustCreateHierarchyTillNullMember()
        {
            // Arrange
            var mapper = new Mapper<Model, AggregationRoot>()
                .Map(e => e.PageData.Model.Display, e => e.Container.Entity.Name);

            var src = fixture.Create<AggregationRoot>();

            src.Container.Entity = null;

            // Act
            var dst = mapper.Map(src);

            // Assert
            dst.PageData.Should().BeNull();
        }

        [Fact]
        public void Map_WhenInvokedForArrayMembers_ShouldMap()
        {
            // Arrange
            var mapper = new Mapper<ArrayRoot, ArrayRootModel>()
                .Map(e => e.Container.Entity.Values, e => e.Page.Model.Src);

            var src = fixture.Create<ArrayRoot>();

            // Act
            var dst = mapper.Map(src);

            // Assert
            dst.Page.Should().NotBeNull();
            dst.Page.Model.Should().NotBeNull();
            dst.Page.Model.Src.Should().NotBeNull();
            dst.Page.Model.Src.Length.Should().Be(src.Container.Entity.Values.Length);
            dst.Page.Model.Src.Should().BeEquivalentTo(src.Container.Entity.Values);
        }

        [Fact]
        public void Map_WhenInvokedForNullArrayMembers_ShouldMap()
        {
            // Arrange
            var mapper = new Mapper<ArrayRoot, ArrayRootModel>()
                .Map(e => e.Container.Entity.Values, e => e.Page.Model.Src);

            var src = fixture.Create<ArrayRoot>();
            src.Container.Entity.Values = null;

            // Act
            var dst = mapper.Map(src);

            // Assert
            dst.Page.Should().NotBeNull();
            dst.Page.Model.Should().NotBeNull();
            dst.Page.Model.Src.Should().BeNull();
        }

        public class A
        {
            public string Name { get; set; }
        }

        public class Container
        {
            public A Entity { get; set; }
        }

        public class AggregationRoot
        {
            public Container Container { get; set; }
        }

        public class AModel
        {
            public string Display { get; set; }
        }

        public class Page
        {
            public AModel Model { get; set; }
        }

        public class Model
        {
            public Page PageData { get; set; }
        }

        public class Entity
        {
            public string[] Values { get; set; }
        }

        public class ArrayContainer
        {
            public Entity Entity { get; set; }
        }

        public class ArrayRoot
        {
            public ArrayContainer Container { get; set; }
        }

        public class ArrayModel
        {
            public string[] Src { get; set; }
        }

        public class ArrayModelContainer
        {
            public ArrayModel Model { get; set; }
        }

        public class ArrayRootModel
        {
            public ArrayModelContainer Page { get; set; }
        }
    }
}