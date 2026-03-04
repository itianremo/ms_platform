using MediatR;
using Apps.Domain.Entities;
using Apps.Application.Features.Apps.Queries.GetAllApps;

namespace Apps.Application.Features.Apps.Queries.GetAppById;

public record GetAppByIdQuery(Guid Id) : IRequest<AppDto?>;
