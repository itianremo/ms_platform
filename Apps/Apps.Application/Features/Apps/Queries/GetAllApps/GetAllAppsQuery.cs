using MediatR;
using Apps.Domain.Entities;

namespace Apps.Application.Features.Apps.Queries.GetAllApps;

public record GetAllAppsQuery : IRequest<List<AppConfig>>;
