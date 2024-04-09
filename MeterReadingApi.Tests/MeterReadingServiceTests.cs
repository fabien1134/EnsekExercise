using MeterReadingApi.Data;
using MeterReadingApi.Models;
using MeterReadingApi.Services;
using Microsoft.AspNetCore.Http;
using Moq;
using static MeterReadingApi.CustomExceptions.CustomExceptions;

namespace MeterReadingApi.Tests
{
    public class MeterReadingServiceTests
    {
        private readonly Mock<IRepository<MeterReading>> _meterReadingRepositoryMock = new();
        private readonly Mock<ICsvHelperService<UnprocessedMeterReading>> _csvHelperServiceMock = new();
        private readonly MeterReadingService _meterReadingService;

        public MeterReadingServiceTests()
        {
            _meterReadingService = new MeterReadingService(_meterReadingRepositoryMock.Object, _csvHelperServiceMock.Object);
        }

        [Fact]
        public void IsCsvFileTypeValid_ReturnsTrue_WhenCsvFileIsValid()
        {
            var csvFileMock = new Mock<IFormFile>();
            csvFileMock.Setup(f => f.ContentType).Returns("text/csv");
            csvFileMock.Setup(f => f.FileName).Returns("test.csv");

            var result = _meterReadingService.IsCsvFileTypeValid(csvFileMock.Object);

            Assert.True(result);
        }

        [Fact]
        public void IsCsvFileTypeValid_ReturnsFalse_WhenCsvFileIsInvalid()
        {
            var csvFileMock = new Mock<IFormFile>();
            csvFileMock.Setup(f => f.ContentType).Returns("text/plain");
            csvFileMock.Setup(f => f.FileName).Returns("test.txt");

            var result = _meterReadingService.IsCsvFileTypeValid(csvFileMock.Object);

            Assert.False(result);
        }

        [Fact]
        public void IsCsvFileUploaded_ReturnsTrue_WhenCsvFileIsUploaded()
        {
            var csvFileMock = new Mock<IFormFile>();
            csvFileMock.Setup(f => f.Length).Returns(100);

            var result = _meterReadingService.IsCsvFileUploaded(csvFileMock.Object);

            Assert.True(result);
        }

        [Fact]
        public void IsCsvFileUploaded_ReturnsFalse_WhenCsvFileIsNotUploaded()
        {
            IFormFile? csvFile = null;

            var result = _meterReadingService.IsCsvFileUploaded(csvFile);

            Assert.False(result);
        }

        [Fact]
        public async Task SaveCsvRecordsAsync_ReturnsCorrectCounts_WhenRecordsAreValid()
        {
            var csvRecords = new List<MeterReading>
            {
                new() { AccountId = 1, MeterReadingDateTimeUtc = DateTime.UtcNow, MeterReadValue = "00001" },
                new() { AccountId = 2, MeterReadingDateTimeUtc = DateTime.UtcNow, MeterReadValue = "00002" }
            };

            _meterReadingRepositoryMock.Setup(r => r.CreateAsync(It.IsAny<MeterReading>())).ReturnsAsync(true);

            var (successfulReadings, failedReadings) = await _meterReadingService.SaveCsvRecordsAsync(csvRecords);

            Assert.Equal(2, successfulReadings);
            Assert.Equal(0, failedReadings);
        }

        [Fact]
        public async Task SaveCsvRecordsAsync_ReturnsCorrectCounts_WhenRecordsAreInvalid()
        {
            var csvRecords = new List<MeterReading>
            {
                new() { AccountId = 1, MeterReadingDateTimeUtc = DateTime.UtcNow, MeterReadValue = "00001" },
                new() { AccountId = 2, MeterReadingDateTimeUtc = DateTime.UtcNow, MeterReadValue = "00002" }
            };

            _meterReadingRepositoryMock.Setup(r => r.CreateAsync(It.IsAny<MeterReading>())).ReturnsAsync(false);

            var (successfulReadings, failedReadings) = await _meterReadingService.SaveCsvRecordsAsync(csvRecords);

            Assert.Equal(0, successfulReadings);
            Assert.Equal(2, failedReadings);
        }

        [Fact]
        public async Task SaveCsvRecordsAsync_ReturnsCorrectCounts_WhenSomeRecordsAreInvalid()
        {
            var csvRecords = new List<MeterReading>
            {
                new() { AccountId = 1, MeterReadingDateTimeUtc = DateTime.UtcNow, MeterReadValue = "00001" },
                new() { AccountId = 2, MeterReadingDateTimeUtc = DateTime.UtcNow, MeterReadValue = "00002" }
            };

            _meterReadingRepositoryMock.SetupSequence(r => r.CreateAsync(It.IsAny<MeterReading>()))
                .ReturnsAsync(true)
                .ReturnsAsync(false);

            var (successfulReadings, failedReadings) = await _meterReadingService.SaveCsvRecordsAsync(csvRecords);

            Assert.Equal(1, successfulReadings);
            Assert.Equal(1, failedReadings);
        }

        [Fact]
        public async Task SaveCsvRecordsAsync_ThrowsException_WhenRepositoryFails()
        {
            var csvRecords = new List<MeterReading>
                            {
                                new() { AccountId = 1, MeterReadingDateTimeUtc = DateTime.UtcNow, MeterReadValue = "00001" }
                            };

            _meterReadingRepositoryMock.Setup(r => r.CreateAsync(It.IsAny<MeterReading>())).Throws<Exception>();

            await Assert.ThrowsAsync<Exception>(() => _meterReadingService.SaveCsvRecordsAsync(csvRecords));
        }

        [Fact]
        public void ReturnValidatedCsvRecords_WhenDuplicateAccountId_ThrowsException()
        {
            // Arrange
            var csvFileMock = new Mock<IFormFile>();
            csvFileMock.Setup(f => f.ContentType).Returns("text/csv");
            csvFileMock.Setup(f => f.FileName).Returns("test.csv");

            var duplicateAccountMeterReadings = new List<UnprocessedMeterReading>
            {
                new() { AccountId = 144, MeterReadingDateTime = "22/04/2019  09:24:00", MeterReadValue = "12345" },
                new() { AccountId = 144, MeterReadingDateTime = "22/04/2019  09:24:00", MeterReadValue = "67890" }
            };

            _csvHelperServiceMock.Setup(s => s.ReturnCsvRecords(It.IsAny<IFormFile>())).Returns(duplicateAccountMeterReadings);

            // Act & Assert
            Assert.Throws<BadRequestException>(() => _meterReadingService.ReturnValidatedCsvRecords(csvFileMock.Object));
        }
    }
}