﻿namespace BackendTrackerPresentation.Graphql.GraphqlTypes;

public class ApplicationUserInput
{
    public string? UserName { get; set; }
    public string? Email { get; set; }
    public string? Password { get; set; }
    public string? Role { get; set; }
}