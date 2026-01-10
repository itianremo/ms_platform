using MediatR;
using Auth.Application.Common.DTOs;

namespace Auth.Application.Features.Auth.Queries.GetAllUsers;

public record GetAllUsersQuery : IRequest<List<UserDto>>;
