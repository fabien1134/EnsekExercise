namespace MeterReadingApi.Services
{
    public interface ICsvHelperService<T> where T : class
    {
        List<T> ReturnCsvRecords(IFormFile csvFile);
    }
}
