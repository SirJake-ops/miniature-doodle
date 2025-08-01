using AutoMapper;
using BackendTrackerApplication.Dtos;
using BackendTrackerApplication.Exceptions;
using BackendTrackerDomain.Interfaces;

namespace BackendTrackerApplication.Services.ApplicationUser;

public class ApplicationUserService(IApplicationUserRepository applicationUserRepository, IMapper mapper)
{
    public async Task<CreateUserRequestDto?> GetUserByIdAsync(Guid userId)
    {
        var user = await applicationUserRepository.GetUserByIdAsync(userId) ??
                   throw new UserNotFoundException(userId.ToString(), new Dictionary<string, string[]>());

        return mapper.Map<CreateUserRequestDto>(user);
    }

    public async Task<CreateUserRequestDto?> UpdateUser(Guid userId, CreateUserRequestDto userRequestDto)
    {
        var user = await applicationUserRepository.GetUserByIdAsync(userId) ??
                   throw new UserNotFoundException(userId.ToString(), new Dictionary<string, string[]>());

        var updatedUser = mapper.Map<BackendTrackerDomain.Entity.ApplicationUser.ApplicationUser>(userRequestDto);


        await applicationUserRepository.UpdateUserAsync(userId, updatedUser);

        return mapper.Map<CreateUserRequestDto>(updatedUser);
    }
}