using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.DTOs;
using TaskFlow.Application.Repositories;
using TaskFlow.Infrastructure.Config;

namespace TaskFlow.Admin.Pages.Projects;

[Authorize]
public class IndexModel : PageModel
{
    private readonly IProjectRepository _projectRepository;

    public IndexModel(IProjectRepository projectRepository)
    {
        _projectRepository = projectRepository;
    }

    public IList<ProjectDetailstViewDTO> Projects { get;set; } = default!;

    public async Task OnGetAsync()
    {
        var projects = await _projectRepository.GetAllAsync();
        Projects = await _projectRepository.EnrichListAsync(projects);
    }
}
