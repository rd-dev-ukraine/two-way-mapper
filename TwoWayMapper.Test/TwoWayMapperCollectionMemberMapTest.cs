using System.Collections.Generic;
using Ploeh.AutoFixture;
using Xunit;

namespace TwoWayMapper.Test
{
    public class TwoWayMapperCollectionMemberMapTest
    {
        private readonly IFixture fixture = new Fixture();

        [Fact]
        public void Map_WhenInvoked_ShouldMapArrayOfPrimitivesCorrectly()
        {
            // Arrange
            var mapper = new Mapper<A, B>()
                .Map((a, i) => a.Data[i], (b, i) => b.Values[i]);

            var src = fixture.Create<B>();

            // Act
            var dst = mapper.Map(src);

            // Assert
            Assert.NotNull(dst.Data);
            Assert.Equal(dst.Data.Length, src.Values.Length);

            for (var i = 0; i < dst.Data.Length; i++)
                Assert.Equal(src.Values[i], dst.Data[i]);
        }

        [Fact]
        public void Map_WhenInvoked_ShouldMapListOfPrimitivesCorrectly()
        {
            // Arrange
            var mapper = new Mapper<A, B>()
                .Map((a, i) => a.ListData[i], (b, i) => b.ListValues[i]);

            var src = fixture.Create<B>();

            // Act
            var dst = mapper.Map(src);

            // Assert
            Assert.NotNull(dst.ListData);
            Assert.Equal(dst.ListData.Count, src.ListValues.Count);

            for (var i = 0; i < dst.ListData.Count; i++)
                Assert.Equal(src.ListValues[i], dst.ListData[i]);
        }

        [Fact]
        public void Map_WhenInvokedForListOfIncompatibleTypes_ShouldMapWithConversion()
        {
            // Arrange
            var mapper = new Mapper<A, B>()
                .Map((a, i) => a.Data[i], (b, i) => b.IntValues[i]);

            var src = fixture.Create<A>();
            for (var i = 0; i < src.Data.Length; i++)
                src.Data[i] = i.ToString();

            // Act
            var dst = mapper.Map(src);

            // Assert
            Assert.NotNull(dst.IntValues);
            Assert.Equal(src.Data.Length, dst.IntValues.Count);
            for(var i = 0; i < dst.IntValues.Count; i++)
                Assert.Equal(i, dst.IntValues[i]);
        }

        [Fact]
        public void Map_WhenInvokedForFromListToListOfIncompatibleTypes_ShouldMapWithConversion()
        {
            // Arrange
            var mapper = new Mapper<A, B>()
                .Map((a, i) => a.ListData[i], (b, i) => b.IntValues[i]);

            var src = fixture.Create<A>();
            for (var i = 0; i < src.ListData.Count; i++)
                src.ListData[i] = i.ToString();

            // Act
            var dst = mapper.Map(src);

            // Assert
            Assert.NotNull(dst.IntValues);
            Assert.Equal(src.ListData.Count, dst.IntValues.Count);
            for (var i = 0; i < dst.IntValues.Count; i++)
                Assert.Equal(i, dst.IntValues[i]);
        }

        [Fact]
        public void Map_WhenMappingListOfObjects_ShouldMapWithConversion()
        {
            // Arrange
            var mapper = new Mapper<ACont, ObjCont>()
                .Map((a, i) => a.A[i], (b, i) => b.Obj[i]);

            var src = fixture.Create<ACont>();
            // Act
            var dst = mapper.Map(src);

            // Assert
            Assert.NotNull(dst.Obj);
            Assert.Equal(src.A.Count, dst.Obj.Count);
            for (var i = 0; i < dst.Obj.Count; i++)
                Assert.Same(src.A[i], dst.Obj[i]);
        }

        [Fact]
        public void Map_WhenMappedNested_ShouldMapCorrectly()
        {
            // Arrange
            var mapper = new Mapper<Nest<A>, Nest<B>>()
                .Map((a, i, j) => a.Nested[i].Data[j], (b, k, l)=> b.Nested[k].ListValues[k]);

            var src = fixture.Create<Nest<A>>();

            // Act
            var dst = mapper.Map(src);

            // Assert
            Assert.NotNull(dst.Nested);
            Assert.Equal(src.Nested.Count, dst.Nested.Count);

            for (var i = 0; i < dst.Nested.Count; i++)
            {
                var s = src.Nested[i];
                var d = dst.Nested[i];

                Assert.Equal(s.Data.Length, d.ListValues.Count);
                for (var j = 0; j < d.ListValues.Count; j++)
                    Assert.Equal(s.Data[j], d.ListValues[j]);
            }
        }

        public class A
        {
            public string[] Data { get; set; }

            public List<string> ListData { get; set; }

            public int[] IntArray { get; set; }
        }

        public class B
        {
            public string[] Values { get; set; }

            public List<string> ListValues { get; set; } 

            public List<int> IntValues { get; set; } 
        }

        public class Nest<T>
        {
            public List<T> Nested { get; set; } 
        }

        public class ACont
        {
            public List<A> A { get; set; } 
        }

        public class ObjCont
        {
            public List<object> Obj { get; set; }
        }
    }
}