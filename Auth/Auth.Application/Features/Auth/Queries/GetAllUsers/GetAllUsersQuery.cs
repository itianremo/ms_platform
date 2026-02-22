using System;
using MediatR;
using Auth.Application.Common.DTOs;

namespace Auth.Application.Features.Auth.Queries.GetAllUsers;

public record GetAllUsersQuery(Guid? AppId = null) : IRequest<List<UserDto>>;
