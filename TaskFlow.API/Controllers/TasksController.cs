using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using TaskFlow.Application.DTOs;
using TaskFlow.Infrastructure.Config;
using TaskFlow.Application.Repositories;
using TaskFlow.Application.Services;
using TaskFlow.Infrastructure.Repositories;

namespace TaskFlow.API.Controllers;

[Authorize("isUser")]
[ApiController]
[Route("taskflow/[controller]")]
public class TasksController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly ITaskItemRepository _taskItemRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly IUserContext _userContext;

    public TasksController(
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

    [HttpGet("mine")]
    public async Task<IActionResult> ListMyTasks()
    {
        var auth = await _userContext.GetAuthorizations(User, null);

        if (auth.UserId is null || !auth.IsUser)
            return Unauthorized();

        var tasks = await _taskItemRepository.GetByUsernameAsync(auth.UserId);

        var tasksEnriched = await _taskItemRepository.EnrichListAsync(tasks);

        return Ok(tasksEnriched);
    }

    [HttpGet("{key}")]
    public async Task<IActionResult> Get([FromRoute] string key)
    {
        var taskItem = await _taskItemRepository.GetByKeyAsync(key);
        var whichProject = taskItem?.Project;

        var auth = await _userContext.GetAuthorizations(User, whichProject?.Key);

        if (!auth.IsOwner & !auth.IsParticipant)
            return Unauthorized();

        return Ok(taskItem);
    }
    [HttpGet("{key}/details")]
    public async Task<IActionResult> Details([FromRoute] string key)
    {
        var taskItem = await _taskItemRepository.GetByKeyAsync(key);
        var whichProject = taskItem?.Project;

        var auth = await _userContext.GetAuthorizations(User, whichProject?.Key);

        if (!auth.IsOwner & !auth.IsParticipant)
            return Unauthorized();

        var taskItemEnriched = await _taskItemRepository.EnrichAsync(taskItem);

        return Ok(taskItemEnriched);
    }

    [HttpPatch("{key}")]
    public async Task<IActionResult> Edit(
        [FromRoute] string key,
        [FromBody] TaskChangeDTO body
    )
    {
        var auth = await _userContext.GetAuthorizations(User, key);

        if (!auth.IsOwner && !auth.IsParticipant && !auth.IsAdmin)
            return Unauthorized();

        var operation = body.operation;
        var value = body.value;

        var operations = new List<string> { "assign", "complete" };

        if (string.IsNullOrWhiteSpace(operation) || 
            !operations.Contains(operation) ||
            operation == operations[0] && value is null)
            return BadRequest("Request parameters could not be resolved");

        var task = await _taskItemRepository.GetByKeyAsync(key);
        var taskUpd = await _taskItemRepository.UpdateAsync(task, body);
        
        return Ok( new { 
            Message = "Changes applied",
            Task = task,
        });
    }
    [HttpPatch("{key}/comment")]
    public async Task<IActionResult> Comment(
        [FromRoute] string key,
        [FromBody] string body
    )
    {
        var auth = await _userContext.GetAuthorizations(User, key);

        if (!auth.IsOwner && !auth.IsParticipant && !auth.IsAdmin)
            return Unauthorized();

        var ok = await _taskItemRepository.CommentAsync(body, key, auth.UserId);

        if (ok is null)
            return BadRequest("Request parameters could not be resolved");
        
        return Ok(ok);
    }
}
