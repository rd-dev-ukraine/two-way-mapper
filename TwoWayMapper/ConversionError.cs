namespace TwoWayMapper
{
    public class ConversionError
    {
        public string SourcePath { get; set; }
        
        public string DestinationPath { get; set; }
        
        public string ErrorMessage { get; set; }

        public object SourceValue { get; set; }
    }
}