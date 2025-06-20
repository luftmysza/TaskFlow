using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskFlow.Domain.Entities;

public class ApplicationUser : IdentityUser
{
    public override string UserName { get; set; } = null!;
    public ICollection<UserProject>? UserProjects { get; set; }
    public ICollection<TaskItem>? AssignedTasks { get; set; }
    public ICollection<Comment>? Comments { get; set; }
    public Stack<Comment>? UnreadComments { get; set; }
}