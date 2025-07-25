using BackendTracker.Auth;
using BackendTrackerApplication.Dtos;

namespace BackendTrackerApplication.Interfaces;

public interface IApplicationUserService
{
    Task<AuthResponse> Login(LoginRequest loginRequest);
}