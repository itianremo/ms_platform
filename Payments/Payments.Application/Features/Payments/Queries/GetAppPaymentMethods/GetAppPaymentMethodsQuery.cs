using MediatR;
using Payments.Domain.Repositories;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace Payments.Application.Features.Payments.Queries.GetAppPaymentMethods
{
    public class GetAppPaymentMethodsQuery : IRequest<List<string>>
    {
        public string AppId { get; set; }
    }

    public class GetAppPaymentMethodsQueryHandler : IRequestHandler<GetAppPaymentMethodsQuery, List<string>>
    {
        private readonly IAppPaymentProviderRepository _repository;

        public GetAppPaymentMethodsQueryHandler(IAppPaymentProviderRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<string>> Handle(GetAppPaymentMethodsQuery request, CancellationToken cancellationToken)
        {
            var providers = await _repository.GetByAppIdAsync(request.AppId);
            return providers.Select(p => p.GatewayName).ToList();
        }
    }
}
