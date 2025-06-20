using System.ComponentModel.DataAnnotations;
using TaskFlow.Domain.Entities;

public class Project
{
    [Key]
    public string Key { get; set; }

    // Navigation: many-to-many with users
    public ICollection<UserProject>? UserProjects { get; set; }

    // One-to-many: tasks in this project
    public ICollection<TaskItem>? Tasks { get; set; }
}
public static class ProjectExtensions
{
    public static string GenerateNextTaskKey(this Project project)
    {
        var count = project.Tasks?.Count ?? 0;
        return $"{project.Key}-{count + 1}";
    }
}
