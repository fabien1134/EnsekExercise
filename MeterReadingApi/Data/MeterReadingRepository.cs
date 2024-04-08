using MeterReadingApi.Models;
using Microsoft.EntityFrameworkCore;

namespace MeterReadingApi.Data
{
    public class MeterReadingRepository(DataContext context) : IRepository<MeterReading>
    {
        private readonly DataContext _context = context;

        public async Task<bool> CreateAsync(MeterReading entity)
        {
            if (entity == null || !await IsValidRecordAsync(entity)) return false;

            return await SaveRecordAsync(entity);
        }

        public async Task<bool> IsValidRecordAsync(MeterReading entity)
        {
            //Ensure the meter reading is associated with an existing account
            if (entity == null || !await _context.Accounts.AnyAsync(m => m.Id == entity.AccountId)) return false;

            //Ensure if an existing meter reading is provided, we only make the entity as valid if it has a greater DateTime
            //Would be more effecient to make it a stored procedure and provide it the Entity UTC DateTime
            if (!await _context.MeterReadings.AnyAsync(m => m.AccountId == entity.AccountId)) return true;

            DateTime exisingReadDateTimeUtc = default;

            exisingReadDateTimeUtc = await _context.MeterReadings.Where(m => m.AccountId == entity.AccountId).MaxAsync(m => m.MeterReadingDateTimeUtc);

            return entity.MeterReadingDateTimeUtc > exisingReadDateTimeUtc;
        }


        public async Task<bool> SaveRecordAsync(MeterReading entity)
        {
            if (entity == null) return false;

            try
            {
                await _context.MeterReadings.AddAsync(entity);

                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }
    }
}
