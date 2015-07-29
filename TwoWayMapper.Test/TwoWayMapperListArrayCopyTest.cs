using System.Collections.Generic;
using System.Linq;
using Ploeh.AutoFixture;
using Xunit;

namespace TwoWayMapper.Test
{
    public class TwoWayMapperListArrayCopyTest
    {
        private readonly IFixture fixture = new Fixture();

        [Fact]
        public void Map_WhenInvoked_ShouldMapArrayMember()
        {
            // Arrange
            var mapper = new Mapper<SArr, DArr>()
                .Map((arr, i) => arr.Src[i].Name, (arr, i) => arr.Dst[i].Name)
                .Map((arr, i) => arr.Src[i].Id, (arr, i) => arr.Dst[i].Id);

            var src = fixture.Create<SArr>();

            // Act
            var dst = mapper.Map(src);

            // Assert
            Assert.NotNull(dst.Dst);
            Assert.Equal(dst.Dst.Count, src.Src.Count);

            for (var i = 0; i < dst.Dst.Count; i++)
            {
                var srcElm = src.Src[i];
                var dstElm = dst.Dst[i];

                Assert.NotNull(dstElm);
                Assert.Equal(srcElm.Name, dstElm.Name);
                Assert.Equal(srcElm.Id, dstElm.Id);
            }
        }

        [Fact]
        public void Map_WhenInvoked_ShouldMapArrayMemberWithNesting()
        {
            // Arrange
            var mapper = new Mapper<Src, DstNest>()
                .Map((a, i, j) => a.S.Source[i].Src[j].Name, (b, i, j) => b.Destination[i].Dst[j].Name)
                .Map((c, i, j) => c.S.Source[i].Src[j].Id, (d, i, j) => d.Destination[i].Dst[j].Id);

            var src = fixture.Create<Src>();

            // Act
            var dst = mapper.Map(src);

            // Assert
            Assert.NotNull(dst.Destination);
            Assert.Equal(dst.Destination.Count, src.S.Source.Count);

            for (var i = 0; i < dst.Destination.Count; i++)
            {
                var s = src.S.Source[i];
                var d = dst.Destination[i];

                Assert.NotNull(d.Dst);
                Assert.Equal(s.Src.Count, d.Dst.Count);

                for (var j = 0; j < d.Dst.Count; j++)
                {
                    var srcElm = s.Src[j];
                    var dstElm = d.Dst[j];

                    Assert.NotNull(dstElm);
                    Assert.Equal(srcElm.Name, dstElm.Name);
                    Assert.Equal(srcElm.Id, dstElm.Id);
                }
            }
        }

        [Fact]
        public void Map_WhenInvokedForNullArray_ShouldNotCreateDest()
        {
            // Arrange
            var mapper = new Mapper<SArr, DArr>()
                .Map(s => s.Counter, s => s.Number)
                .Map((s, i) => s.Src[i].Name, (s, i) => s.Dst[i].Name);

            var src = fixture.Create<DArr>();
            src.Dst = null;

            // Act
            var dst = mapper.Map(src);

            // Assert
            Assert.Equal(src.Number, dst.Counter);
            Assert.Null(dst.Src);
        }

        [Fact]
        public void Map_WhenInvokedForNullNestedArray_ShouldMapCorrectly()
        {
            // Arrange
            var mapper = new Mapper<SrcNest, DstNest>()
                .Map((e, i, j) => e.Source[i].Src[j].Name, (e, i, j) => e.Destination[i].Dst[j].Name);

            var src = new SrcNest
            {
                Source = fixture.CreateMany<SArr>(2).ToList()
            };
            src.Source[0].Src = null;

            // Act
            var dst = mapper.Map(src);

            // Assert
            Assert.Equal(src.Source.Count, dst.Destination.Count);
            Assert.Null(dst.Destination[0].Dst);
        }

        [Fact]
        public void Map_WhenInvokedForNullArrayElement_ShouldMapCorrectly()
        {
            // Arrange
            var mapper = new Mapper<SrcNest, DstNest>()
                .Map((e, i, j) => e.Source[i].Src[j].Name, (e, i, j) => e.Destination[i].Dst[j].Name);

            var src = new SrcNest
            {
                Source = fixture.CreateMany<SArr>(2).ToList()
            };
            src.Source[0] = null;

            // Act
            var dst = mapper.Map(src);

            // Assert
            Assert.Equal(src.Source.Count, dst.Destination.Count);
            Assert.Null(dst.Destination[0]);
        }

        public class Src
        {
            public SrcNest S { get; set; }
        }

        public class SrcNest
        {
            public  List<SArr> Source { get; set; }
        }

        public class Dst
        {
            public List<DstNest> Nest { get; set; }
        }

        public class DstNest
        {
            public List<DArr> Destination { get; set; }
        }

        public class SArr
        {
            public int Counter { get; set; }

            public List<Elm> Src { get; set; }
        }

        public class DArr
        {
            public int Number { get; set; }

            public List<Elm> Dst { get; set; }
        }

        public class Elm
        {
            public string Name { get; set; }

            public string Id { get; set; }
        }
    }
}