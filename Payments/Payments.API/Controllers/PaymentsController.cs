using Microsoft.AspNetCore.Mvc;
using MediatR;
using Payments.Application.Features.Payments.Commands.SubscribeUser;
using System.Threading.Tasks;

namespace Payments.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public PaymentsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("config/{appId}")]
        public async Task<IActionResult> GetAvailableMethods(string appId)
        {
            var query = new Payments.Application.Features.Payments.Queries.GetAppPaymentMethods.GetAppPaymentMethodsQuery { AppId = appId };
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpPost("config")]
        public async Task<IActionResult> ConfigureMethod([FromBody] Payments.Application.Features.Payments.Commands.ConfigureAppPaymentMethod.ConfigureAppPaymentMethodCommand command)
        {
            await _mediator.Send(command);
            return Ok();
        }

        [HttpPost("subscribe")]
        public async Task<IActionResult> Subscribe([FromBody] SubscribeUserCommand command)
        {
            var result = await _mediator.Send(command);
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }
            return Ok(result);
        }
    }
}
