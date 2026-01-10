using MediatR;

namespace Media.Application.Features.Media.Commands.UploadMedia;

public record UploadMediaCommand(Stream FileStream, string FileName, string ContentType) : IRequest<string>;
