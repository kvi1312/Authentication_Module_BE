using MediatR;

namespace Authentication.Application.Queries;

public class ValidateTokenQuery : IRequest<bool>
{
    public string Token { get; set; } = default!;
    public string TokenType { get; set; } = default!; // access, refresh, remember
}