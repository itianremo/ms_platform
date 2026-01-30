using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Payments.Infrastructure.Persistence;

public class PaymentsDbInitializer
{
    private readonly PaymentsDbContext _context;
    private readonly ILogger<PaymentsDbInitializer> _logger;

    public PaymentsDbInitializer(PaymentsDbContext context, ILogger<PaymentsDbInitializer> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task InitializeAsync()
    {
        try
        {
            // Handle race conditions where container might not be ready or DB already exists
            try
            {
                // EnsureCreatedAsync helps in dev/test for quick startup without migrations tool
                await _context.Database.EnsureCreatedAsync(); 
            }
            catch (Exception ex)
            {
                // If it exists, we might just log and continue, or if using Migrations, apply them.
                // For this environment, we are favoring EnsureCreated.
                _logger.LogWarning("Database might already exist or EnsureCreated failed: {Message}", ex.Message);
            }

            // Cleanup legacy "system-global" duplicated seeds if they exist
            // Provide a way to clean them up so they don't appear as duplicates
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM AppPaymentProviders WHERE AppId = 'system-global'");

            /* 
             * Obsolete Seeding - Removed to prevent duplicates
             * Now we rely on Dynamic App Configuration via the Frontend/API
            var defaultConfigs = new List<Payments.Domain.Entities.AppPaymentProvider>
            {
                new(Guid.NewGuid(), "system-global", "Stripe", true, "{\"publishableKey\":\"\",\"secretKey\":\"\"}"),
                // ...
            };

            foreach (var config in defaultConfigs)
            {
                if (!await _context.AppPaymentProviders.AnyAsync(p => p.GatewayName == config.GatewayName))
                {
                    _context.AppPaymentProviders.Add(config);
                }
            }
            */

             // Seed Banks
             var banks = new List<Payments.Domain.Entities.Bank>
             {
                 new(Guid.NewGuid(), "National Bank of Egypt (NBE)", "EG", "NBEGxCX"),
                 new(Guid.NewGuid(), "Banque Misr", "EG", "BMIXEGCX"),
                 new(Guid.NewGuid(), "Commercial International Bank (CIB)", "EG", "CIBEGcx"),
                 new(Guid.NewGuid(), "QNB Alahli", "EG", "QNBAEGcx"),
                 new(Guid.NewGuid(), "HSBC Bank Egypt", "EG", "HSBCEGcx"),
                 new(Guid.NewGuid(), "Faisal Islamic Bank of Egypt", "EG", "FIEGcx"),
                 new(Guid.NewGuid(), "Alex Bank", "EG", "ALEXEGcx"),
                 new(Guid.NewGuid(), "Crédit Agricole Egypt", "EG", "CRAEGcx"),
                 new(Guid.NewGuid(), "Emirates NBD Egypt", "EG", "EBILcx"),
                 new(Guid.NewGuid(), "Abu Dhabi Islamic Bank (ADIB)", "EG", "ADIBcx"),
                 new(Guid.NewGuid(), "The United Bank", "EG", "UBEGcx"),
                 new(Guid.NewGuid(), "Housing and Development Bank (HDB)", "EG", "HDBKcx"),
                 new(Guid.NewGuid(), "Egyptian Gulf Bank (EGBANK)", "EG", "EGBKcx"),
                 new(Guid.NewGuid(), "SAIB Bank", "EG", "SAIBcx"),
                 new(Guid.NewGuid(), "Arab African International Bank (AAIB)", "EG", "AAIBcx"),
                 new(Guid.NewGuid(), "First Abu Dhabi Bank (FAB)", "EG", "FABEGcx")
             };

             foreach (var bank in banks)
             {
                 if (!await _context.Banks.AnyAsync(b => b.Name == bank.Name))
                 {
                     _context.Banks.Add(bank);
                 }
             }

            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while initializing the database.");
            // Don't throw, allow app to start so fallback works
        }
    }
}
