using System.Security.Claims;
using System.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Domain.Entities;
using TaskFlow.Application.DTOs;
using TaskFlow.Application.Repositories;
using TaskFlow.Application.Services;
using TaskFlow.Infrastructure.Config;
using TaskFlow.Infrastructure.Repositories;

namespace TaskFlow.API.Controllers;

[Authorize(Policy = "IsUser")]
[ApiController]
[Route("taskflow/[controller]")]
public class ProjectsController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly ITaskItemRepository _taskItemRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly IUserContext _userContext;

    public ProjectsController(
        IProjectRepository projectRepository,
        ITaskItemRepository taskItemRepository, 
        IUserRepository userRepository,
        IUserContext userContext
    )
    {
        _userRepository = userRepository;
        _taskItemRepository = taskItemRepository;
        _projectRepository = projectRepository;
        _userContext = userContext;
    }

    [HttpGet("list")]
    public async Task<IActionResult> List()
    {
        var auth = await _userContext.GetAuthorizations(User, null);

        if (auth.IsAdmin)
        {
            var result = _projectRepository.GetAllAsync();
               
            return Ok(result);
        }
        else
        {
            var result = _projectRepository.GetAllByUserIdAsync(auth.UserId);

            return Ok(result);
        }
    }

    [HttpGet("{key}")]
    public async Task<IActionResult> Details([FromRoute] string key)
    {
        var auth = await _userContext.GetAuthorizations(User, null);

        if (!auth.IsOwner && !auth.IsParticipant && !auth.IsAdmin)
            return Unauthorized();

        var result = _projectRepository.GetByIdWithDetailsAsync(key);

        return Ok(result);
    }

    [HttpPost("create")]
    public async Task<IActionResult> Create([FromBody] NewProjectDTO body)
    {
        var auth = await _userContext.GetAuthorizations(User, null);
        
        string key = body.projectKey.ToUpper();
        List<string> usernames = body.userNames;

        bool keyExists = await _projectRepository.ExistsAsync(key);
        if (keyExists)
            return BadRequest("Project key already in use!");

        Project addProject = new Project { Key = key };

        List<ApplicationUser>? found = await _userRepository.GetListByUsernameAsync(usernames);
        List<string> foundUsernames = found.Select(f => f.UserName).ToList();
        var missingUsernames = usernames.Except(foundUsernames).ToList();

        ICollection<UserProject> addUserProject = found.Select(u => new UserProject
        {
            User = u,
            Project = addProject,
            Role = ProjectRole.Participant
        }
        ).ToList();

        addProject.UserProjects = addUserProject;

        await _projectRepository.AddAsync(addProject);

        return Ok(new
        {
            result = "Project created",
            NewKey = key,
            UsersAdded = foundUsernames,
            UsersNotFound = missingUsernames
        });
    }

    [HttpDelete("{key}/delete")]
    public async Task<IActionResult> Delete([FromRoute] string key)
    {
        var auth = await _userContext.GetAuthorizations(User, null);

        if (!auth.IsOwner && !auth.IsAdmin) return Unauthorized();

        key = key.ToUpper();
        Project? deleteProject = await _projectRepository.GetByIdAsync(key);

        if (deleteProject is null)
            return BadRequest("Project does not exist!");

        await _projectRepository.DeleteAsync(deleteProject);

        return Ok(new
        {
            result = "project deleted",
            OldKey = key
        });
    }
    [HttpPost("{key}/AddTask")]
    public async Task<IActionResult> AddTask(
        [FromRoute] string key,
        [FromBody] TaskItemCreateDTO body
        )
    {
        var auth = await _userContext.GetAuthorizations(User, key);

        if (!auth.IsOwner && !auth.IsParticipant && !auth.IsAdmin)
            return Unauthorized();

        string? created = await _taskItemRepository.AddAsync(body, key);

        if (created is null)
            return BadRequest("Parameters could not be resolved");
        
        return Ok(new
        {
            result = "TaskItem created",
            Key = created
        });
    }
}
