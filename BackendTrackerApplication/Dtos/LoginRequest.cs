﻿namespace BackendTracker.Auth;

public class LoginRequest
{
    public string? UserName { get; set; }
    public string? Password { get; set; }
}