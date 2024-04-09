using MeterReadingApi.Controllers;
using MeterReadingApi.Models;
using MeterReadingApi.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace MeterReadingApi.Tests
{
    public class MeterReadingControllerTests
    {
        private readonly MeterReadingController _controller;
        private readonly Mock<ILogger<MeterReadingController>> _loggerMock = new();
        private readonly Mock<IMeterReadingService<MeterReading>> _meterReadingServiceMock = new();

        public MeterReadingControllerTests()
        {
            _controller = new MeterReadingController(_loggerMock.Object, _meterReadingServiceMock.Object);
        }

        [Fact]
        public async Task MeterReadingUploads_ReturnsBadRequest_WhenNoFileUploaded()
        {
            // Arrange
            IFormFile? file = null;

            // Act
            var result = await _controller.MeterReadingUploads(file);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task MeterReadingUploads_ReturnsBadRequest_WhenInvalidFileType()
        {
            // Arrange
            var fileMock = new Mock<IFormFile>();
            var content = "Account Id,Meter Reading Date Time,Meter Read Value";
            var fileName = "test.txt";
            var ms = new MemoryStream();
            var writer = new StreamWriter(ms);
            writer.Write(content);
            writer.Flush();
            ms.Position = 0;
            fileMock.Setup(_ => _.OpenReadStream()).Returns(ms);
            fileMock.Setup(_ => _.FileName).Returns(fileName);
            fileMock.Setup(_ => _.Length).Returns(ms.Length);

            _meterReadingServiceMock.Setup(m => m.IsCsvFileTypeValid(It.IsAny<IFormFile>())).Returns(false);

            // Act
            var result = await _controller.MeterReadingUploads(fileMock.Object);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task MeterReadingUploads_ReturnsOk_WhenValidFileUploaded()
        {
            // Arrange
            var fileMock = new Mock<IFormFile>();
            var content = "Account Id,Meter Reading Date Time,Meter Read Value";
            var fileName = "test.csv";
            var ms = new MemoryStream();
            var writer = new StreamWriter(ms);
            writer.Write(content);
            writer.Flush();
            ms.Position = 0;
            fileMock.Setup(_ => _.OpenReadStream()).Returns(ms);
            fileMock.Setup(_ => _.FileName).Returns(fileName);
            fileMock.Setup(_ => _.Length).Returns(ms.Length);

            _meterReadingServiceMock.Setup(m => m.IsCsvFileUploaded(It.IsAny<IFormFile>())).Returns(true);
            _meterReadingServiceMock.Setup(m => m.IsCsvFileTypeValid(It.IsAny<IFormFile>())).Returns(true);

            _meterReadingServiceMock.Setup(m => m.ReturnValidatedCsvRecords(It.IsAny<IFormFile>())).Returns((new List<MeterReading>(), 0));
            _meterReadingServiceMock.Setup(m => m.SaveCsvRecordsAsync(It.IsAny<List<MeterReading>>())).ReturnsAsync((1, 0));

            // Act
            var result = await _controller.MeterReadingUploads(fileMock.Object);

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }
    }
}
