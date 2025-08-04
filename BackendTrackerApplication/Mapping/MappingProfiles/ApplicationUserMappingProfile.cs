using AutoMapper;
using BackendTracker.Auth;
using BackendTrackerApplication.Dtos;
using BackendTrackerDomain.Entity.ApplicationUser;

namespace BackendTrackerApplication.Mapping.MappingProfiles;

public class ApplicationUserMappingProfile : Profile
{
    public ApplicationUserMappingProfile()
    {
        CreateMap<ApplicationUser, ApplicationUserDto>()
            .ForAllMembers(dest => dest.Condition((src, des, srcDest) => src != null));
        CreateMap<ApplicationUserDto, ApplicationUser>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Password, opt => opt.Ignore())
            .ForMember(dest => dest.Role, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.NewGuid()))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.Messages, opt => opt.Ignore())
            .ForMember(dest => dest.Conversations, opt => opt.Ignore())
            .ForMember(dest => dest.SubmittedTickets, opt => opt.Ignore())
            .ForMember(dest => dest.AssignedTickets, opt => opt.Ignore())
            .ForMember(dest => dest.RefreshToken, opt => opt.Ignore())
            .ForMember(dest => dest.RefreshTokenExpiryTime, opt => opt.Ignore())
            .ForMember(dest => dest.LastLoginTime, opt => opt.Ignore())
            .ForMember(dest => dest.IsOnline, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Token, opt => opt.Ignore());
    }
}