namespace MeterReadingApi.Models
{
    public class MeterReading
    {
        public long Id { get; set; }
        public required long AccountId { get; set; }
        public required DateTime MeterReadingDateTimeUtc { get; set; }
        public required string MeterReadValue { get; set; }
    }
}
