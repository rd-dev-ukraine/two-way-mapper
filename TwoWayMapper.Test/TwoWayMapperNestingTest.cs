using Ploeh.AutoFixture;
using Xunit;

namespace TwoWayMapper.Test
{
    public class TwoWayMapperNestingTest
    {
        private readonly IFixture fixture = new Fixture();

        [Fact]
        public void Map_WhenInvoked_ShouldMapLeftToRight()
        {
            // Arrange
            var map = new Mapper<A, B>()
                .Map(e => e.Id, e => e.Identifier)
                .Map(e => e.Name, e => e.DisplayName);

            var a = fixture.Create<A>();
            var b = new B();

            // Act
            map.LeftToRight(a, b);

            // Assert
            Assert.Equal(b.Identifier, a.Id);
            Assert.Equal(b.DisplayName, a.Name);
        }

        [Fact]
        public void Map_WhenInvoked_ShouldMapRightToLeft()
        {
            // Arrange
            var map = new Mapper<A, B>()
                .Map(e => e.Id, e => e.Identifier)
                .Map(e => e.Name, e => e.DisplayName);

            var b = fixture.Create<B>();
            var a = new A();

            // Act
            map.RightToLeft(b, a);

            // Assert
            Assert.Equal(a.Id, b.Identifier);
            Assert.Equal(a.Name, b.DisplayName);
        }

        [Fact]
        public void Map_WhenInvoked_ShouldMapNestedPathes()
        {
            // Arrange
            var mapper = new Mapper<C, D>()
                .Map(e => e.A.Id, e => e.B.Identifier)
                .Map(e => e.A.Name, e => e.B.DisplayName);

            var c = fixture.Create<C>();

            // Act
            var d = mapper.Map(c);

            // Assert
            Assert.Equal(d.B.Identifier, c.A.Id);
            Assert.Equal(d.B.DisplayName, c.A.Name);
        }

        [Fact]
        public void Map_WhenInvokedForNestedPath_ShouldNotCreateNestedInstanceIfAlreadyInitialized()
        {
            // Arrange
            var mapper = new Mapper<C, D>()
                .Map(e => e.A.Id, e => e.B.Identifier)
                .Map(e => e.A.Name, e => e.B.DisplayName);

            var c = fixture.Create<C>();
            var dest = new D
            {
                B = new B()
            };

            var originalB = dest.B;

            // Act
            mapper.LeftToRight(c, dest);

            // Assert
            Assert.Same(originalB, dest.B);
        }

        [Fact]
        public void Map_WhenInvokedOnFewLevelOfObjectNesting_ShouldCreateAll()
        {
            // Arrange
            var mapper = new Mapper<R, R1>()
                .Map(l => l.C.A.Id, o => o.DVal.B.Identifier)
                .Map(l => l.C.A.Name, o => o.DVal.B.DisplayName)
                .Map(m => m.C.OtherA.Name, v => v.DVal.Other.DisplayName)
                .Map(m => m.C.OtherA.Id, v => v.DVal.Other.Identifier);

            var src = fixture.Create<R>();

            // Act
            var dst = mapper.Map(src);

            // Assert
            Assert.NotNull(dst.DVal);
            Assert.NotNull(dst.DVal.B);
            Assert.Equal(dst.DVal.B.DisplayName, src.C.A.Name);
            Assert.Equal(dst.DVal.B.Identifier, src.C.A.Id);

            Assert.NotNull(dst.DVal.Other);
            Assert.Equal(dst.DVal.Other.DisplayName, src.C.OtherA.Name);
            Assert.Equal(dst.DVal.Other.Identifier, src.C.OtherA.Id);
        }

        [Fact]
        public void Map_WhenMappedAtDifferentPath_ShouldMapCorrectly()
        {
            // Arrange
            var mapper = new Mapper<C, D>()
                .Map(a => a.A.Id, b => b.B.Identifier)
                .Map(c => c.OtherA.Name, d => d.B.DisplayName);

            var src = fixture.Create<D>();

            // Act
            var dst = mapper.Map(src);

            // Assert
            Assert.Equal(dst.A.Id, src.B.Identifier);
            Assert.Equal(dst.OtherA.Name, src.B.DisplayName);
        }

        [Fact]
        public void Map_WhenMappedToSamePathFromTheDifferentPath_ShouldMapCorrectly()
        {
            // Arrange
            var mapper = new Mapper<C, D>()
                .Map(a => a.A.Id, b => b.B.Identifier)
                .Map(c => c.A.Name, d => d.Other.DisplayName);

            var src = fixture.Create<D>();

            // Act
            var dst = mapper.Map(src);

            // Assert
            Assert.Equal(dst.A.Id, src.B.Identifier);
            Assert.Equal(dst.A.Name, src.Other.DisplayName);
        }

        [Fact]
        public void Map_WhenPathToSourceIsNull_ShouldNotMapDest()
        {
            // Arrange
            var mapper = new Mapper<C, D>()
                .Map(a => a.A.Id, b => b.B.Identifier)
                .Map(a => a.A.Name, b => b.B.DisplayName)
                .Map(a => a.OtherA.Id, b => b.Other.Identifier)
                .Map(a => a.OtherA.Name, b => b.Other.DisplayName);

            var src = fixture.Create<D>();
            src.Other = null;

            // Act
            var dst = mapper.Map(src);

            // 
            Assert.NotNull(dst.A);
            Assert.Null(dst.OtherA);
            Assert.Equal(src.B.Identifier, dst.A.Id);
            Assert.Equal(src.B.DisplayName, dst.A.Name);
        }

        [Fact]
        public void Map_WhenPathToSourceIsEitherNullOrObject_ShouldMapNonNullSource()
        {
            // Arrange
            var mapper = new Mapper<C, D>()
                .Map(a => a.A.Id, b => b.B.Identifier)
                .Map(a => a.A.Name, b => b.Other.DisplayName);

            var src = fixture.Create<D>();
            src.Other = null;

            // Act
            var dst = mapper.Map(src);

            // 
            Assert.NotNull(dst.A);
            Assert.Null(dst.OtherA);
            Assert.Equal(src.B.Identifier, dst.A.Id);
            Assert.Null(dst.A.Name);
        }
    }

    internal class A
    {
        public string Id { get; set; }

        public string Name { get; set; }
    }

    internal class B
    {
        public string Identifier { get; set; }

        public string DisplayName { get; set; }
    }

    internal class C
    {
        public A A { get; set; }

        public A OtherA { get; set; }
    }

    internal class D
    {
        public B B { get; set; }

        public B Other { get; set; }
    }

    internal class R
    {
        public C C { get; set; }
    }

    internal class R1
    {
        public D DVal { get; set; }
    }
}