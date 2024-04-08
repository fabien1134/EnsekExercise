namespace MeterReadingApi.Services
{
    public interface IMeterReadingService<T> where T : class
    {
        Task<(long SuccessfulReadings, long FailedReadings)> SaveCsvRecordsAsync(List<T> csvRecords);
        bool IsCsvFileTypeValid(IFormFile csvFile);
        bool IsCsvFileUploaded(IFormFile csvFile);
        (List<T> MeterReadings, long InvalidReadings) ReturnValidatedCsvRecords(IFormFile csvFile);
    }
}
