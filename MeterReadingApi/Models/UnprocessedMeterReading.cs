namespace MeterReadingApi.Models
{
    public class UnprocessedMeterReading
    {
        public required long AccountId { get; set; }
        public required string MeterReadingDateTime { get; set; }
        public required string MeterReadValue { get; set; }
    }
}
