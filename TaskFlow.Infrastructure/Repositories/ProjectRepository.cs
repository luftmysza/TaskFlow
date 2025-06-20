using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.DTOs;
using TaskFlow.Application.Repositories;
using TaskFlow.Domain.Entities;
using TaskFlow.Infrastructure.Config;

namespace TaskFlow.Infrastructure.Repositories;

public class ProjectRepository : IProjectRepository
{
    private readonly AppDbContext _context;
    private readonly IUserRepository _userRepository;

    public ProjectRepository(AppDbContext context, IUserRepository userRepository)
    {
        _context = context;
        _userRepository = userRepository;
    }
    public async Task AddAsync(Project project)
    {
        await _context.AddAsync(project);
        await _context.SaveChangesAsync();
    }
    public async Task AddAsync(ProjectListViewDTO projectDTO)
    {
        Project project = new Project()
        {
            Key = projectDTO.ProjectKey.ToUpper(),
        };
        
        List<ApplicationUser> owners = await _userRepository.GetListByUsernameAsync(projectDTO.Owners);
        
        List<UserProject> userProjects = new List<UserProject>();
        foreach (var owner in owners) 
        {
            var temp = new UserProject()
            {
                Project = project,
                User = owner,
                Role = ProjectRole.Owner
            };

            userProjects.Add(temp);
        }
        project.UserProjects = userProjects;

        await _context.AddAsync(project);
        await _context.SaveChangesAsync();
    }
    public async Task<int> CountAsync()
    {
        return await _context.Projects.CountAsync();
    }

    public async Task DeleteAsync(Project project)
    {
        _context.Projects.Remove(project);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<ProjectListViewDTO>?> GetAllAsync()
    {
        IEnumerable<ProjectListViewDTO>? result = await _context.Projects
         .Select(p => new ProjectListViewDTO
         {
             ProjectKey = p.Key,
             Owners = p.UserProjects
             .Where(up => up.Role == ProjectRole.Owner)!
             .Select(up => up.User.UserName).ToList()
         })
         .ToListAsync();

        return result;
    }

    public async Task<IEnumerable<ProjectListViewDTO>?> GetAllByUserIdAsync(string username)
    {
        IEnumerable<ProjectListViewDTO>? result = await _context.Projects
         .Where(p => p.UserProjects.Any(up => up.User.UserName == username))
         .Select(p => new ProjectListViewDTO
         {
             ProjectKey = p.Key,
             Owners = p.UserProjects
             .Where(up => up.Role == ProjectRole.Owner)!
             .Select(up => up.User.UserName).ToList()
         })
         .ToListAsync();
        
        return result;
    }

    public async Task<Project?> GetByIdAsync(string projectKey)
    {
        Project? result = result = await _context.Projects
            .FirstOrDefaultAsync(p => p.Key == projectKey);

        return result;
    }
    public async Task<ProjectDetailstViewDTO?> GetByIdWithDetailsAsync(string projectKey)
    {
        ProjectDetailstViewDTO? result = await _context.Projects
            .Where(p => p.Key == projectKey)
            .Select(p => new ProjectDetailstViewDTO
            {
                ProjectKey = p.Key,
                Participants = p.UserProjects.Select(up => new ParticipantDTO
                {
                    User = up.User.UserName,
                    Role = up.Role.ToString()
                }).ToList(),
                Tasks = p.Tasks.Select(t => new TaskItemDTO
                {
                    TaskKey = t.TaskKey,
                    Title = t.Title,
                    StatusText = t.StatusText,
                    IsCompleted = t.IsCompleted,
                    Assignee = t.Assignee == null ? "" : t.Assignee.UserName,
                    CreatedAt = t.CreatedAt,
                    DoneAt = t.DoneAt
                }).ToList()
            })
            .FirstOrDefaultAsync();

        return result;
    }

    public async Task UpdateAsync(Project project)
    {
        throw new NotImplementedException();
    }

    public async Task AddParticipantAsync(Project project, ApplicationUser user, ProjectRole role)
    {
        project.UserProjects.Add(new UserProject
        {
            Project = project,
            User = user,
            Role = role
        });
        await _context.SaveChangesAsync();
    }
    public async Task AddParticipantAsync(ProjectListViewDTO projectDTO, ApplicationUser user, ProjectRole role)
    {
        Project? project = await this.GetByIdAsync(projectDTO.ProjectKey);
        if (project is null)
            return;

        project.UserProjects.Add(new UserProject
        {
            Project = project,
            User = user,
            Role = role
        });

        await _context.SaveChangesAsync();
    }
    public async Task<bool> UserHasAccessAsync(string username, string projectKey)
    {
        Project? project = await this.GetByIdAsync(projectKey);
        ApplicationUser? user = await _userRepository.GetByUsernameAsync(username);

        if (project is null)
            return false;

        bool result = await _context.UserProjects.AnyAsync(up =>
           up.Project == project &&
           up.User == user);

        return result;
    }
    public async Task<bool> ExistsAsync(string projectKey)
    {
        bool result = await _context.Projects.Select(p => p.Key).ContainsAsync(projectKey);
        return result;
    }

    public async Task<ProjectDetailstViewDTO>? EnrichAsync(Project project)
    {
        var enriched = await this.GetByIdWithDetailsAsync(project.Key);
        return enriched;
    }
    public async Task<List<ProjectDetailstViewDTO>>? EnrichListAsync(IEnumerable<ProjectListViewDTO> projects)
    {
        var enrichTasks = projects.Select(p => GetByIdWithDetailsAsync(p.ProjectKey)).ToList();
        var result = await Task.WhenAll(enrichTasks);
     
        return result.ToList();
    }
}
