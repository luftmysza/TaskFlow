
using TaskFlow.Domain.Entities;
using TaskFlow.Application.Repositories;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.DTOs;
using TaskFlow.Infrastructure.Config;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace TaskFlow.Infrastructure.Repositories;
public class TaskItemRepository : ITaskItemRepository
{
    private readonly AppDbContext _context;
    private readonly IUserRepository _userRepository;
    private readonly IProjectRepository _projectRepository;
    public TaskItemRepository(AppDbContext context, IUserRepository userRepository, IProjectRepository projectRepository)
    {
        _context = context;
        _userRepository = userRepository;
        _projectRepository = projectRepository;
    }
    public async Task AddAsync(TaskItem task)
    {
        await _context.TaskItems.AddAsync(task);
        await _context.SaveChangesAsync();
    }
    public async Task<string?> AddAsync(TaskItemCreateDTO taskDTO, string projectKey)
    {
        Project? project = await _projectRepository.GetByIdAsync(projectKey);
        if (project is null) 
            return null;
        ApplicationUser? assignee  = await _userRepository.GetByUsernameAsync(taskDTO.Assignee);

        TaskItem task = new()
        {
            TaskKey = project.GenerateNextTaskKey(),
            Title = taskDTO.Title,
            Description = taskDTO.Description,
            Project = project,
            Assignee = assignee
        };

        await _context.TaskItems.AddAsync(task);
        await _context.SaveChangesAsync();

        return task.TaskKey;
    }
    public async Task<int> CountAsync()
    {
        return await _context.TaskItems.CountAsync();
    }

    public async Task DeleteAsync(TaskItem task)
    {
        _context.TaskItems.Remove(task);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<TaskItemDTO>>? GetAllByProjectKeyAsync(string projectKey)
    {
        Project? project = await _projectRepository.GetByIdAsync(projectKey);

        List<TaskItemDTO>? result = project?.Tasks.Select(t => new TaskItemDTO
        {
            TaskKey = t.TaskKey,
            Title = t.Title,
            StatusText = t.StatusText,
            IsCompleted = t.IsCompleted,
            Assignee = t.Assignee.UserName,
            CreatedAt = t.CreatedAt,
            DoneAt = t.DoneAt
        }).ToList();

        return result;
    }

    public async Task<TaskItem?> GetByKeyAsync(string taskKey)
    {
        TaskItem? result = await _context.TaskItems
            .Include(ti => ti.Project)
            .Include(ti => ti.Assignee)
            .FirstOrDefaultAsync(ti => ti.TaskKey == taskKey);

        return result;
    }
    public async Task<List<TaskItem>?> GetByUsernameAsync(string username)
    {
        List<TaskItem>? result = await _context.TaskItems
            .Where(ti => ti.Assignee.UserName == username).ToListAsync();

        return result;
    }

    public async Task<TaskItem?> UpdateAsync(TaskItem task, TaskChangeDTO body)
    {
        TaskItem? result = await GetByKeyAsync(task.TaskKey);
        
        switch (body.operation)
        {
            case "assign":
                ApplicationUser user = await _userRepository.GetByUsernameAsync(body.value!);
                result!.Assignee = user;
                break;
            case "complete":
                result!.Status = TaskFlow.Domain.Entities.TaskStatus.New;
                result.IsCompleted = true;
                result.DoneAt = DateTime.UtcNow; ;
                break;
            default: break;
        }

        await _context.SaveChangesAsync();

        return result;
    }
    public async Task<Comment?> CommentAsync(string text, string taskKey, string username)
    {
        ApplicationUser? user = await _userRepository.GetByUsernameAsync(username);
        TaskItem? taskObject = await GetByKeyAsync(taskKey);
        if (user is null || taskObject is null)
            return null;

        Comment comment = new Comment()
        {
            Text = text,
            User = user,
            Task = taskObject
        };

        await _context.Comments.AddAsync(comment);

        await NotifyAssignee(comment, taskObject, user);

        await _context.SaveChangesAsync();

        return comment;
    }

    private async Task NotifyAssignee(Comment comment, TaskItem task, ApplicationUser user)
    {
        if (user == task?.Assignee) return;

        task?.Assignee?.UnreadComments?.Push(comment);
        await _context.SaveChangesAsync();
    }

    public async Task<List<TaskItemDetailsDTO>>? EnrichListAsync(IEnumerable<TaskItem> tasks)
    {

        var enriched = tasks.Select(t => EnrichAsync(t));

        var result = await Task.WhenAll(enriched);
        return result.ToList();
    }
    public async Task<TaskItemDetailsDTO>? EnrichAsync(TaskItem task)
    {
        TaskItemDetailsDTO? result = await _context.TaskItems
            .Where(t => t.TaskKey == task.TaskKey)
            .Include(t => t.Comments)
            .Select(t => new TaskItemDetailsDTO
            {
                TaskKey = t.TaskKey,
                Title = t.Title,
                Description = t.Description,
                StatusText = t.StatusText,
                IsCompleted = t.IsCompleted,
                Assignee = t.Assignee == null ? "" : t.Assignee.UserName,
                CreatedAt = t.CreatedAt,
                DoneAt = t.DoneAt,
                Comments = t.Comments == null ? new List<Comment>() : t.Comments.ToList()
            })
            .FirstOrDefaultAsync();

        return result;
    }
}