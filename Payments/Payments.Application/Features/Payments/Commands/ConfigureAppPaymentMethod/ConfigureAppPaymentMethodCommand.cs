using MediatR;
using Payments.Domain.Entities;
using Payments.Domain.Repositories;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Payments.Application.Features.Payments.Commands.ConfigureAppPaymentMethod
{
    public class ConfigureAppPaymentMethodCommand : IRequest<bool>
    {
        public string AppId { get; set; }
        public string GatewayName { get; set; }
        public bool IsEnabled { get; set; }
        public string ConfigJson { get; set; }
    }

    public class ConfigureAppPaymentMethodCommandHandler : IRequestHandler<ConfigureAppPaymentMethodCommand, bool>
    {
        private readonly IAppPaymentProviderRepository _repository;

        public ConfigureAppPaymentMethodCommandHandler(IAppPaymentProviderRepository repository)
        {
            _repository = repository;
        }

        public async Task<bool> Handle(ConfigureAppPaymentMethodCommand request, CancellationToken cancellationToken)
        {
            var existing = await _repository.GetAsync(request.AppId, request.GatewayName);

            if (existing != null)
            {
                existing.IsEnabled = request.IsEnabled;
                existing.ConfigJson = request.ConfigJson;
                await _repository.UpdateAsync(existing);
            }
            else
            {
                var newConfig = new AppPaymentProvider
                {
                    AppId = request.AppId,
                    GatewayName = request.GatewayName,
                    IsEnabled = request.IsEnabled,
                    ConfigJson = request.ConfigJson
                };
                await _repository.AddAsync(newConfig);
            }

            return true;
        }
    }
}
