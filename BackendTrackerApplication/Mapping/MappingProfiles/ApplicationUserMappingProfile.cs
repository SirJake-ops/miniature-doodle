using AutoMapper;
using BackendTracker.Auth;
using BackendTrackerApplication.Dtos;
using BackendTrackerDomain.Entity.ApplicationUser;

namespace BackendTrackerApplication.Mapping.MappingProfiles;

public class ApplicationUserMappingProfile : Profile
{
    public ApplicationUserMappingProfile()
    {
        CreateMap<ApplicationUser, CreateUserRequestDto>();
        CreateMap<ApplicationUser, LoginRequest>();
        CreateMap<CreateUserRequestDto, ApplicationUser>();
    }
}