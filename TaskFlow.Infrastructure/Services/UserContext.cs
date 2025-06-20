using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using TaskFlow.Application.DTOs;
using TaskFlow.Application.Services;

namespace TaskFlow.Infrastructure.Services;

public class UserContext : IUserContext
{
    private readonly IServiceProvider _provider;
    public UserContext(IServiceProvider provider)
    {
        _provider = provider;
    }

    public async Task<UserContextDTO> GetAuthorizations(ClaimsPrincipal user, string? projectKey)
    {
        IAuthorizationService? _authService =  _provider.GetService<IAuthorizationService>();
        UserContextDTO result = new UserContextDTO();

        if (_authService is null) return result;

        var ID = user.FindFirstValue(ClaimTypes.Name);
        var isAdmin = await _authService.AuthorizeAsync(user, "isAdmin");
        var isUser = await _authService.AuthorizeAsync(user, "isUser");
        var isOwner = await _authService.AuthorizeAsync(user, projectKey, "isOwner");
        var isParticipant = await _authService.AuthorizeAsync(user, projectKey, "isParticipant");

        result = new UserContextDTO
        {
            UserId = ID,
            IsAdmin = isAdmin.Succeeded,
            IsUser = isUser.Succeeded,
            IsOwner = isOwner.Succeeded,
            IsParticipant = isParticipant.Succeeded,
        };

        return result;
    }
}
