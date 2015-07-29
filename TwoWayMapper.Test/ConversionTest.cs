using System.Linq;
using Ploeh.AutoFixture;
using TwoWayMapper.Conversion;
using Xunit;

namespace TwoWayMapper.Test
{
    public class ConversionTest
    {
        private readonly IFixture fixture = new Fixture();

        [Fact]
        public void Map_WhenConversionRequired_ShouldTryMapUsingDefaultConverter()
        {
            // Arrange
            var mapper = new Mapper<A, B>()
                .Map(a => a.Num, e => e.String)
                .Map(a => a.NumStr, e => e.StrNum);

            var src = new B
            {
                String = "1",
                StrNum = 12
            };

            // Act
            var dst = mapper.Map(src);

            // Assert
            Assert.Equal(dst.Num, 1);
            Assert.Equal(dst.NumStr, "12");
        }

        [Fact]
        public void Map_WhenConversionRequired_ShouldConvert()
        {
            // Arrange
            var mapper = new Mapper<A, B>()
                .Map(a => a.Num, e => e.String)
                .Map(a => a.NumStr, e => e.StrNum);

            var src = new B
            {
                String = "1",
                StrNum = 12
            };

            // Act
            var dst = mapper.Map(src);

            // Assert
            Assert.Equal(dst.Num, 1);
            Assert.Equal(dst.NumStr, "12");
        }


        [Fact]
        public void Map_WhenConversionFailed_ShouldThrowConversionError()
        {
            // Arrange
            var mapper = new Mapper<A, B>()
                .Map(a => a.Num, e => e.String);

            var src = new B
            {
                String = "err"
            };

            // Throws
            try
            {
                var dst = mapper.Map(src);
                Assert.True(false);
            }
            catch (ConversionException ex)
            {
                Assert.Equal(1, ex.Errors.Length);
                var err = ex.Errors[0];

                Assert.Equal("String", err.SourcePath);
                Assert.Equal("Num", err.DestinationPath);
                Assert.NotEmpty(err.ErrorMessage);
                Assert.Equal("err", err.SourceValue);
            }
        }

        [Fact]
        public void Map_WhenConversionFailedForNestedArray_ShouldNotThrowErrorForNullPathes()
        {
            // Arrange
            var mapper = new Mapper<NestArr<A>, NestArr<B>>()
                .Map((a, i, j) => a.Nest[i].Val[j].Num, (a, i, j) => a.Nest[i].Val[j].String);

            var src = fixture.Build<NestArr<B>>()
                           .With(a => a.Nest,
                                 fixture.Build<Arr<B>>()
                                        .With(e => e.Val,
                                              fixture.Build<B>()
                                                     .With(b => b.String, "3456")
                                                     .CreateMany(4)
                                                     .ToArray())
                                        .CreateMany(3)
                                        .ToArray())
                           .Create();

            src.Nest[1] = null;
            src.Nest[2].Val = null;

            // Act
            var dst = mapper.Map(src);

            // Assert
            Assert.NotNull(dst);
            Assert.NotNull(dst.Nest);
            Assert.Equal(dst.Nest.Length, src.Nest.Length);
            Assert.Null(dst.Nest[1]);
            Assert.Null(dst.Nest[2].Val);
        }

        [Fact]
        public void Map_WhenConversionFailedForNestedArray_ShouldThrowCorrectError()
        {
            // Arrange
            var mapper = new Mapper<NestArr2<A>, NestArr<B>>()
                .Map((a, i, j) => a.Inner[i].Val[j].Num, (a, i, j) => a.Nest[i].Val[j].String);

            var src = new NestArr<B>
            {
                Nest = new Arr<B>[4]
            };
            for (var i = 0; i < src.Nest.Length; i++)
            {
                src.Nest[i] = new Arr<B>();
                src.Nest[i].Val = new B[5];
                for (var j = 0; j < src.Nest[i].Val.Length; j++)
                    src.Nest[i].Val[j] = new B
                    {
                        String = "5545"
                    };
            }

            src.Nest[2].Val[1].String = "Err";
            src.Nest[3].Val[2].String = "incorrect";

            // Throws
            try
            {
                var dst = mapper.Map(src);
                Assert.True(false);
            }
            catch (ConversionException ex)
            {
                Assert.Equal(2, ex.Errors.Length);

                var err = ex.Errors[0];
                Assert.Equal("Nest[2].Val[1].String", err.SourcePath);
                Assert.Equal("Inner[2].Val[1].Num", err.DestinationPath);
                Assert.NotEmpty(err.ErrorMessage);
                Assert.Equal("Err", err.SourceValue);

                var err2= ex.Errors[1];
                Assert.Equal("Nest[3].Val[2].String", err2.SourcePath);
                Assert.Equal("Inner[3].Val[2].Num", err2.DestinationPath);
                Assert.NotEmpty(err2.ErrorMessage);
                Assert.Equal("incorrect", err2.SourceValue);
            }
        }


        [Fact]
        public void Map_ByteToInt_ShouldConvert()
        {
            // Arrange
            var mapper = new Mapper<ConvertibleData, ConvertibleData>()
                .Map(s => s.Byte, s => s.Int);

            var src = fixture.Create<ConvertibleData>();

            // Act

            var dst = new ConvertibleData();
            mapper.LeftToRight(src, dst);

            // Assert
            Assert.Equal(src.Byte, dst.Int);
        }

        [Fact]
        public void Map_IntToByte_ShouldConvert()
        {
            // Arrange
            var mapper = new Mapper<ConvertibleData, ConvertibleData>()
                .Map(s => s.Int, s => s.Byte);

            var src = fixture.Create<ConvertibleData>();

            // Act

            var dst = new ConvertibleData();
            mapper.LeftToRight(src, dst);

            // Assert
            Assert.Equal(src.Int, dst.Byte);
        }

        public class A
        {
            public int Num { get; set; }

            public string NumStr { get; set; }
        }

        public class B
        {
            public string String { get; set; }

            public int StrNum { get; set; }

            public B Copy() => (B)MemberwiseClone();
        }

        public class ConvertibleData
        {
            public int Int { get; set; }

            public bool Bool { get; set; }

            public byte Byte { get; set; }
        }

        public class Arr<T>
        {
            public T[] Val { get; set; }
        }

        public class NestArr<T>
        {
            public Arr<T>[] Nest { get; set; }
        }

        public class NestArr2<T>
        {
            public Arr<T>[] Inner { get; set; }
        }
    }
}