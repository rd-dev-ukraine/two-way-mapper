namespace TwoWayMapper.Conversion
{
    public interface IConverter<TSource, TDest>
    {
        ConversionResult<TSource, TDest> Convert(TSource value);
    }
}