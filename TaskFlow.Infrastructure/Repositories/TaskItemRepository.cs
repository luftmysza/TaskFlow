
using TaskFlow.Domain.Entities;
using TaskFlow.Application.Repositories;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.DTOs;
using TaskFlow.Infrastructure.Config;

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
}