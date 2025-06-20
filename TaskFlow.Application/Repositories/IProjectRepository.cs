using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskFlow.Application.DTOs;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Repositories;

public interface IProjectRepository
{
    public Task<Project?> GetByIdAsync(string projectKey);
    public Task<IEnumerable<ProjectListViewDTO>?> GetAllAsync();
    public Task<ProjectDetailstViewDTO?> GetByIdWithDetailsAsync(string projectKey);
    public Task<IEnumerable<ProjectListViewDTO>?> GetAllByUserIdAsync(string username);
    public Task AddAsync(Project project);
    public Task AddAsync(ProjectListViewDTO projectDTO);
    public Task<int> CountAsync();
    public Task UpdateAsync(Project project);
    public Task DeleteAsync(Project project);
    public Task<bool> UserHasAccessAsync(string username, string projectKey);
    public Task<bool> ExistsAsync(string projectKey);
    public Task AddParticipantAsync(Project project, ApplicationUser user, ProjectRole role);
    public Task AddParticipantAsync(ProjectListViewDTO projectDTO, ApplicationUser user, ProjectRole role);
    public Task<ProjectDetailstViewDTO>? EnrichAsync(Project project);
    public Task<List<ProjectDetailstViewDTO>>? EnrichListAsync(IEnumerable<ProjectListViewDTO> projects);
}
