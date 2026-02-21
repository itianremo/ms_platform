using MediatR;
using System;
using System.Collections.Generic;

namespace Users.Application.Queries.GetReportReasons;

public record GetReportReasonsQuery(Guid AppId) : IRequest<List<string>>;
