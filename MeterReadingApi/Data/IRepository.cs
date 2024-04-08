namespace MeterReadingApi.Data
{
    public interface IRepository<T> where T : class
    {
        Task<bool> CreateAsync(T entity);

        Task<bool> IsValidRecordAsync(T entity);

        Task<bool> SaveRecordAsync(T entity);
    }
}
