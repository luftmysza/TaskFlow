﻿namespace TaskFlow.Application.DTOs;

public class NewProjectDTO
{
    public string projectKey { get; set; } = null!;
    public List<string> userNames { get; set; } = null!;
}
