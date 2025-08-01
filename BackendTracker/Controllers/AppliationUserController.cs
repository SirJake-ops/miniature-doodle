using BackendTrackerApplication.Dtos;
using BackendTrackerApplication.Services.ApplicationUser;
using BackendTrackerInfrastructure.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace BackendTrackerPresentation.Controllers;

[Route("api/users")]
public class AppliationUserController(ApplicationUserService userService) : ControllerBase
{
   [HttpGet("{userId}")]
   public async Task<IActionResult> GetUser([FromRoute] Guid userId)
   {
      var user = await userService.GetUserByIdAsync(userId);
      return Ok(user);
   }

   [HttpPost("{userId}")]
   public async Task<IActionResult> UpdateUser([FromRoute] Guid userId, [FromBody] CreateUserRequestDto userRequestDto)
   {
       var user = await userService.UpdateUser(userId, userRequestDto);
       return Ok(user);
   }
   
   
}