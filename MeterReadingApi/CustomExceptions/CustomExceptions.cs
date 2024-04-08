namespace MeterReadingApi.CustomExceptions
{
    public class CustomExceptions
    {
        public class BadRequestException(string message) : Exception(message) { }
    }
}
