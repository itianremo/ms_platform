using MediatR;
using Shared.Kernel;
using Users.Domain.Repositories;
using System.Linq;

namespace Users.Application.Queries.GetReportByUserIds;

public record UserReportDto(string Reason, string? Comment);

public record GetReportByUserIdsQuery(Guid AppId, Guid ReporterId, Guid ReportedId) : IRequest<UserReportDto?>;

public class GetReportByUserIdsQueryHandler : IRequestHandler<GetReportByUserIdsQuery, UserReportDto?>
{
    private readonly IUserReportRepository _repository;

    public GetReportByUserIdsQueryHandler(IUserReportRepository repository)
    {
        _repository = repository;
    }

    public async Task<UserReportDto?> Handle(GetReportByUserIdsQuery request, CancellationToken cancellationToken)
    {
        var existingReports = await _repository.ListAsync(r => 
            r.ReporterId == request.ReporterId && 
            r.ReportedId == request.ReportedId && 
            r.AppId == request.AppId);
            
        var existingReport = existingReports.FirstOrDefault();
        if (existingReport == null) return null;
        return new UserReportDto(existingReport.ReasonText, existingReport.Comment);
    }
}
