using MediatR;
using Apps.Domain.Entities;

namespace Apps.Application.Features.Apps.Queries.GetAppById;

public record GetAppByIdQuery(Guid Id) : IRequest<AppConfig?>;
