using System.Collections.Generic;
using System.Security.Claims;
using TaskFlow.Application.Repositories;
using TaskFlow.Application.Services;
using TaskFlow.Domain.Entities;
using TaskFlow.Infrastructure.Repositories;
using TaskFlow.Infrastructure.Services;


namespace TaskFlow.SOAP.Services;

public class CommentsSoapService : ICommentsSoapService
{
    private readonly IUserRepository _userRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ITokenCreator _tokenCreator;
    public CommentsSoapService(
        IUserRepository userRepository,
        IHttpContextAccessor httpContextAccessor,
        ITokenCreator tokenCreator
    )
    {
        _httpContextAccessor = httpContextAccessor;
        _userRepository = userRepository;
        _tokenCreator = tokenCreator;
    }
    public async Task<List<Comment>> DumpCommentsAsync()
    {
        ApplicationUser user = await AuthenticateAsync();

        List<Comment> result = new();

        while (user.UnreadComments.Count > 0)
            result.Add(user.UnreadComments.Pop());

        return result;
    }
    private async Task<ApplicationUser> AuthenticateAsync()
    {
        var authHeader = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].FirstOrDefault();
        
        if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            throw new UnauthorizedAccessException("Missing or malformed Authorization header.");
        var token = authHeader["Bearer ".Length..].Trim();

        var claimsPrincipal = await _tokenCreator.DecodeToken(token);
        string? userName = claimsPrincipal.FindFirst(ClaimTypes.Name)?.Value;
        ApplicationUser? user = await _userRepository.GetByUsernameAsync(userName);

        if (user is null)
            throw new UnauthorizedAccessException("Token is invalid.");

        return user;
    }
}
