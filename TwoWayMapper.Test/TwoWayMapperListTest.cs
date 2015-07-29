using System.Collections.Generic;
using Ploeh.AutoFixture;
using Xunit;

namespace TwoWayMapper.Test
{
    public class TwoWayMapperListTest
    {
        private readonly IFixture fixture = new Fixture();

        [Fact]
        public void Map_WhenInvoked_ShouldMapListMember()
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
        public void Map_WhenInvoked_ShouldMapListMemberWithNesting()
        {
            // Arrange
            var mapper = new Mapper<Src, Dst>()
                .Map((a, i, j) => a.S.Source[i].Src[j].Name, (b, i, j) => b.Nest.Destination[i].Dst[j].Name)
                .Map((c, i, j) => c.S.Source[i].Src[j].Id, (d, i, j) => d.Nest.Destination[i].Dst[j].Id);

            var src = fixture.Create<Src>();

            // Act
            var dst = mapper.Map(src);

            // Assert
            Assert.NotNull(dst.Nest);
            Assert.NotNull(dst.Nest.Destination);
            Assert.Equal(dst.Nest.Destination.Count, src.S.Source.Length);

            for (var i = 0; i < dst.Nest.Destination.Count; i++)
            {
                var s = src.S.Source[i];
                var d = dst.Nest.Destination[i];

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


        public class Src
        {
            public SrcNest S { get; set; }
        }

        public class SrcNest
        {
            public SArr[] Source { get; set; }
        }

        public class Dst
        {
            public DstNest Nest { get; set; }
        }

        public class DstNest
        {
            public List<DArr> Destination { get; set; }
        }

        public class SArr
        {
            public List<Elm> Src { get; set; }
        }

        public class DArr
        {
            public List<Elm> Dst { get; set; }
        }

        public class Elm
        {
            public string Name { get; set; }

            public string Id { get; set; }
        }
    }


}