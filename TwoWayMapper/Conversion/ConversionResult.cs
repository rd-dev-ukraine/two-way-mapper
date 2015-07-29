namespace TwoWayMapper.Conversion
{
    public class ConversionResult<TSource, TDest>
    {
        private ConversionResult() { } 

        public bool IsConvertedSuccessfully { get; set; }

        public TSource SourceValue { get; set; }

        public TDest ConvertedValue { get; set; }

        public string ErrorMessage { get; set; }

        /// <summary>
        /// Gets or sets a suffix to a source path using for clarify the member where error is occured.
        /// </summary>
        public string SourcePathSuffix { get; set; }

        public static ConversionResult<TSource, TDest> Ok(TDest convertedValue, TSource sourceValue)
        {
            return new ConversionResult<TSource, TDest>
            {
                IsConvertedSuccessfully = true,
                SourceValue = sourceValue,
                ConvertedValue = convertedValue
            };
        }

        public static ConversionResult<TSource, TDest> Fail(string errorMessage, TSource sourceValue, string sourcePathSuffix = null)
        {
            return new ConversionResult<TSource, TDest>
            {
                IsConvertedSuccessfully = false,
                ErrorMessage = errorMessage,
                SourcePathSuffix = sourcePathSuffix,
                SourceValue = sourceValue
            };
        } 
    }
}