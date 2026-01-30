using Microsoft.EntityFrameworkCore;
using Payments.Domain.Entities;
using Payments.Domain.Repositories;
using Payments.Infrastructure.Persistence;
using Shared.Kernel;
using System.Linq.Expressions;
using System.Reflection;

namespace Payments.Infrastructure.Repositories
{
    public class AppPaymentProviderRepository : IAppPaymentProviderRepository
    {
        private readonly PaymentsDbContext _context;
        private static List<AppPaymentProvider> _memoryStore = new();
        private static bool _seeded = false;
        private static readonly object _lock = new();

        public AppPaymentProviderRepository(PaymentsDbContext context)
        {
            _context = context;
        }

        private void SetId(Entity entity, Guid id)
        {
            var prop = typeof(Entity).GetProperty(nameof(Entity.Id));
            if (prop != null)
            {
                 prop.SetValue(entity, id);
            }
        }

        private async Task EnsureSeededAsync()
        {
            if (_seeded) return;
            
            try
            {
                var seedPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Persistence", "Seed", "payment_configs.json");
                if (File.Exists(seedPath))
                {
                    var json = await File.ReadAllTextAsync(seedPath);
                    var configs = System.Text.Json.JsonSerializer.Deserialize<List<AppPaymentProvider>>(json);
                    
                    lock (_lock)
                    {
                        if (!_seeded && configs != null)
                        {
                            foreach(var c in configs) 
                            { 
                                if (c.Id == Guid.Empty) SetId(c, Guid.NewGuid());
                            }
                            _memoryStore.AddRange(configs);
                            _seeded = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to seed memory store: {ex.Message}");
            }
        }

        public async Task<List<AppPaymentProvider>> GetByAppIdAsync(string appId)
        {
            try
            {
                return await _context.AppPaymentProviders
                    .Where(x => x.AppId == appId)
                    .ToListAsync();
            }
            catch
            {
                await EnsureSeededAsync();
                lock(_lock) {
                    return _memoryStore.Where(x => x.AppId == appId).ToList();
                }
            }
        }

        public async Task<AppPaymentProvider?> GetAsync(string appId, string gatewayName)
        {
             try
            {
                return await _context.AppPaymentProviders
                    .FirstOrDefaultAsync(x => x.AppId == appId && x.GatewayName == gatewayName);
            }
            catch
            {
                await EnsureSeededAsync();
                lock(_lock) {
                    return _memoryStore.FirstOrDefault(x => x.AppId == appId && x.GatewayName == gatewayName);
                }
            }
        }

        public async Task<AppPaymentProvider> AddAsync(AppPaymentProvider entity)
        {
             try
            {
                 _context.AppPaymentProviders.Add(entity);
                 await _context.SaveChangesAsync();
                 return entity;
            }
            catch
            {
                await EnsureSeededAsync();
                lock(_lock) {
                     if (entity.Id == Guid.Empty) SetId(entity, Guid.NewGuid());
                    _memoryStore.Add(entity);
                    return entity;
                }
            }
        }

        public async Task UpdateAsync(AppPaymentProvider entity)
        {
             try
            {
                 _context.Entry(entity).State = EntityState.Modified;
                 await _context.SaveChangesAsync();
            }
            catch
            {
                await EnsureSeededAsync();
                lock(_lock) {
                    var existing = _memoryStore.FirstOrDefault(x => x.Id == entity.Id);
                    if (existing != null)
                    {
                        existing.IsEnabled = entity.IsEnabled;
                        existing.ConfigJson = entity.ConfigJson;
                        existing.GatewayName = entity.GatewayName;
                        existing.AppId = entity.AppId;
                    }
                    else if (entity.Id == Guid.Empty) 
                    {
                        var logical = _memoryStore.FirstOrDefault(x => x.AppId == entity.AppId && x.GatewayName == entity.GatewayName);
                         if (logical != null)
                        {
                            logical.IsEnabled = entity.IsEnabled;
                            logical.ConfigJson = entity.ConfigJson;
                        }
                    }
                }
            }
        }

        public async Task DeleteAsync(AppPaymentProvider entity)
        {
             try
            {
                 _context.AppPaymentProviders.Remove(entity);
                 await _context.SaveChangesAsync();
            }
            catch
            {
                 lock(_lock) {
                    var existing = _memoryStore.FirstOrDefault(x => x.Id == entity.Id);
                    if (existing != null) _memoryStore.Remove(existing);
                 }
            }
        }

        public async Task<AppPaymentProvider?> GetByIdAsync(Guid id)
        {
             try
             {
                 return await _context.AppPaymentProviders.FindAsync(id);
             }
             catch
             {
                 await EnsureSeededAsync();
                 lock(_lock) return _memoryStore.FirstOrDefault(x => x.Id == id);
             }
        }
        
        public async Task<List<AppPaymentProvider>> ListAsync()
        {
             try
             {
                 return await _context.AppPaymentProviders.ToListAsync();
             }
             catch
             {
                 await EnsureSeededAsync();
                 lock(_lock) return _memoryStore.ToList();
             }
        }

        public async Task<List<AppPaymentProvider>> ListAsync(Expression<Func<AppPaymentProvider, bool>> predicate)
        {
             try
             {
                 return await _context.AppPaymentProviders.Where(predicate).ToListAsync();
             }
             catch
             {
                 await EnsureSeededAsync();
                 var func = predicate.Compile();
                 lock(_lock) return _memoryStore.Where(func).ToList();
             }
        }
    }
}
