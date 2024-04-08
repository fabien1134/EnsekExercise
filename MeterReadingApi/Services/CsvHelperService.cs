using CsvHelper;
using CsvHelper.Configuration;
using MeterReadingApi.Models;
using System.Globalization;

namespace MeterReadingApi.Services
{
    public class CsvHelperService : ICsvHelperService<UnprocessedMeterReading>
    {
        public List<UnprocessedMeterReading> ReturnCsvRecords(IFormFile csvFile)
        {
            using var reader = new StreamReader(csvFile.OpenReadStream());
            using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                PrepareHeaderForMatch = args => args.Header.ToLower(),
                MissingFieldFound = null
            });

            return csv.GetRecords<UnprocessedMeterReading>().ToList();
        }
    }
}
