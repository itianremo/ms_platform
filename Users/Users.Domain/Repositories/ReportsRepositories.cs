using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Shared.Kernel;
using Users.Domain.Entities;

namespace Users.Domain.Repositories;

public interface IReportReasonRepository : IRepository<ReportReason>
{
    Task<List<ReportReason>> GetActiveReasonsAsync(Guid appId);
}

public interface IUserReportRepository : IRepository<UserReport>
{
}
