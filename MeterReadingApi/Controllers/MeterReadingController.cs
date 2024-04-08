using MeterReadingApi.Models;
using MeterReadingApi.Services;
using Microsoft.AspNetCore.Mvc;
using static MeterReadingApi.CustomExceptions.CustomExceptions;

namespace MeterReadingApi.Controllers
{
    [Route("/meter-reading-uploads")]
    [ApiController]
    public class MeterReadingController(ILogger<MeterReadingController> logger, IMeterReadingService<MeterReading> meterReadingService) : ControllerBase
    {
        private readonly ILogger<MeterReadingController> _logger = logger;
        private readonly IMeterReadingService<MeterReading> _meterReadingService = meterReadingService;

        [HttpPost(Name = "meter-reading-uploads")]
        public async Task<IActionResult> MeterReadingUploads(IFormFile csvFile)
        {
            try
            {
                // Validate the file (e.g., size, extension)
                if (!_meterReadingService.IsCsvFileUploaded(csvFile)) return BadRequest("No file uploaded.");

                if (!_meterReadingService.IsCsvFileTypeValid(csvFile)) return BadRequest("Invalid file type. Only CSV files are allowed.");

                var (meterReadings, invalidReadings) = _meterReadingService.ReturnValidatedCsvRecords(csvFile);

                var (successfulReadings, failedSavedReadings) = await _meterReadingService.SaveCsvRecordsAsync(meterReadings);

                return Ok((successfulReadings, invalidReadings + failedSavedReadings));
            }
            catch (BadRequestException ex)
            {
                // Handle the exception here
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error processing CSV file: {ex.Message}");
            }
        }
    }
}
