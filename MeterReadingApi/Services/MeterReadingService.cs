using MeterReadingApi.Data;
using MeterReadingApi.Models;
using static MeterReadingApi.CustomExceptions.CustomExceptions;
using System.Globalization;
using System.Text.RegularExpressions;

namespace MeterReadingApi.Services
{
    public class MeterReadingService(IRepository<MeterReading> meterReadingRepository, ICsvHelperService<UnprocessedMeterReading> csvHelperService) : IMeterReadingService<MeterReading>
    {
        private readonly IRepository<MeterReading> _meterReadingRepository = meterReadingRepository;
        private readonly ICsvHelperService<UnprocessedMeterReading> _csvHelperService = csvHelperService;

        private const string meterReadingValidationRegex = @"^-?\d{1,4}$|^\d{1,5}$";
        private const string csvContentType = "text/csv";
        private const string csvPathExtension = ".csv";
        private const string textNegativeSybal = "-";
        private const char textZeroCharacter = '0';

        public virtual bool IsCsvFileTypeValid(IFormFile csvFile) => (csvFile.ContentType == csvContentType && Path.GetExtension(csvFile.FileName).Equals(csvPathExtension, StringComparison.CurrentCultureIgnoreCase));

        public virtual bool IsCsvFileUploaded(IFormFile csvFile) => csvFile != null && csvFile.Length > 0;

        public (List<MeterReading> MeterReadings, long InvalidReadings) ReturnValidatedCsvRecords(IFormFile csvFile)
        {
            var unprocessedMeterReadings = _csvHelperService.ReturnCsvRecords(csvFile);

            var meterReadings = new List<MeterReading>();

            var failedReadings = 0;

            var accountIds = new HashSet<long>();

            foreach (var unprocessedMeterReading in unprocessedMeterReadings)
            {
                if (accountIds.Contains(unprocessedMeterReading.AccountId)) throw new BadRequestException($"Duplicate accountId {unprocessedMeterReading.AccountId} found in CSV file");

                accountIds.Add(unprocessedMeterReading.AccountId);

                if (unprocessedMeterReading.AccountId <= 0)
                {
                    failedReadings++;
                    continue;
                }

                if (!DateTime.TryParseExact(unprocessedMeterReading.MeterReadingDateTime, "dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var meterReadingDateTime))
                {
                    failedReadings++;
                    continue;
                }

                var (IsMeterReadingValid, FormatedMeterReading) = ValidateAndFormatMeterReadingIfValid(unprocessedMeterReading.MeterReadValue);

                if (!IsMeterReadingValid)
                {
                    failedReadings++;
                    continue;
                }

                meterReadings.Add(new MeterReading
                {
                    AccountId = unprocessedMeterReading.AccountId,
                    MeterReadingDateTimeUtc = meterReadingDateTime.ToUniversalTime(),
                    MeterReadValue = FormatedMeterReading
                });
            }

            return (meterReadings, failedReadings);
        }

        public async Task<(long SuccessfulReadings, long FailedReadings)> SaveCsvRecordsAsync(List<MeterReading> csvRecords)
        {
            var successfulReadings = 0;
            var failedReadings = 0;

            var accountIds = new HashSet<long>();

            foreach (var csvRecord in csvRecords)
            {
                if (!accountIds.Contains(csvRecord.AccountId) && await _meterReadingRepository.CreateAsync(csvRecord))
                {
                    successfulReadings++;
                    accountIds.Add(csvRecord.AccountId);
                }
                else
                {
                    failedReadings++;
                }
            }

            return (successfulReadings, failedReadings);
        }


        private (bool IsMeterReadingValid, string FormatedMeterReading) ValidateAndFormatMeterReadingIfValid(string meterReading)
        {
            if (string.IsNullOrWhiteSpace(meterReading)) return (false, string.Empty);

            string? formattedMeterReading = null;
            bool isMatch = false;

            try
            {
                isMatch = new Regex(meterReadingValidationRegex).IsMatch(meterReading);

                if (isMatch)
                {
                    //We want to format the numbers so there is always 5 digits e.g 56 = 00056 or -56 = -00056
                    formattedMeterReading = meterReading.StartsWith(textNegativeSybal)
                    ? textNegativeSybal + meterReading.TrimStart('-').PadLeft(5, textZeroCharacter)
                    : meterReading.PadLeft(5, textZeroCharacter);
                }
            }
            catch (Exception)
            {

            }

            return (isMatch, formattedMeterReading ?? meterReading);
        }
    }
}
