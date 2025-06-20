using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskFlow.Application.DTOs;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Repositories;

public interface ITaskItemRepository
{
    public Task<TaskItem?> GetByKeyAsync(string taskKey);
    public Task<List<TaskItem>?> GetByUsernameAsync(string username);
    public Task<IEnumerable<TaskItemDTO>>? GetAllByProjectKeyAsync(string projectKey);
    public Task AddAsync(TaskItem task);
    public Task<int> CountAsync();
    public Task<TaskItem?> UpdateAsync(TaskItem task, TaskChangeDTO body);
    public Task DeleteAsync(TaskItem task);
}