using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Payments.Infrastructure.Persistence;

namespace Payments.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BanksController : ControllerBase
    {
        private readonly PaymentsDbContext _context;

        public BanksController(PaymentsDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetBanks()
        {
            var banks = await _context.Banks
                .OrderBy(b => b.Name)
                .Select(b => new { b.Id, b.Name, b.SwiftCode })
                .ToListAsync();
            return Ok(banks);
        }
    }
}
