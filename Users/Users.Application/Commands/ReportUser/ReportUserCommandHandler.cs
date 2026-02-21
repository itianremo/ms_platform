using MediatR;
using System.Threading;
using System.Threading.Tasks;
using Users.Domain.Entities;
using Users.Domain.Repositories;

namespace Users.Application.Commands.ReportUser;

public class ReportUserCommandHandler : IRequestHandler<ReportUserCommand, bool>
{
    private readonly IUserReportRepository _repository;

    public ReportUserCommandHandler(IUserReportRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> Handle(ReportUserCommand request, CancellationToken cancellationToken)
    {
        var existingReports = await _repository.ListAsync(r => 
            r.ReporterId == request.ReporterId && 
            r.ReportedId == request.ReportedId && 
            r.AppId == request.AppId);
            
        var existingReport = existingReports.FirstOrDefault();

        if (existingReport != null)
        {
            existingReport.ReasonText = request.Reason;
            existingReport.Comment = request.Comment;
            await _repository.UpdateAsync(existingReport);
            return true;
        }
        else
        {
            var report = new UserReport
            {
                AppId = request.AppId,
                ReporterId = request.ReporterId,
                ReportedId = request.ReportedId,
                ReasonText = request.Reason,
                Comment = request.Comment
            };

            var added = await _repository.AddAsync(report);
            return added != null;
        }
    }
}
