using MediatR;
using System;

namespace Users.Application.Commands.ReportUser;

public record ReportUserCommand(Guid AppId, Guid ReporterId, Guid ReportedId, string Reason, string? Comment = null) : IRequest<bool>;
