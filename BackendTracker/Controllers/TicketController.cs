using BackendTracker.Ticket.NewFolder;
using BackendTracker.Ticket.PayloadAndResponse;
using BackendTrackerApplication.DTOs;
using BackendTrackerApplication.Services;
using BackendTrackerPresentation.Graphql.Subscriptions;
using HotChocolate.Subscriptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BackendTrackerPresentation.Controllers;

[ApiController]
[Route("api/tickets")]
public class TicketController(TicketService ticketService) : Controller
{
    [HttpGet("{ticketId:guid}")]
    [Authorize]
    public async Task<ActionResult<BackendTrackerDomain.Entity.Ticket.Ticket>> GetTicketById(Guid ticketId)
    {
        var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(idStr, out var viewerId)) return Unauthorized();

        var role = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;
        var isAdmin = string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase);

        var ticket = await ticketService.GetTicketById(ticketId);
        if (ticket == null) return NotFound();

        if (!isAdmin && ticket.SubmitterId != viewerId && ticket.AssigneeId != viewerId) return Forbid();

        return Ok(ticket);
    }

    [HttpGet]
    [Authorize]
    public async Task<IEnumerable<BackendTrackerDomain.Entity.Ticket.Ticket>> GetTickets([FromQuery] Guid? submitterId = null)
    {
        var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        Guid.TryParse(idStr, out var viewerId);
        var role = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;
        var isAdmin = string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase);

        return await ticketService.GetTicketsForViewer(viewerId, isAdmin, submitterId);
    }

    [HttpPost]
    [Authorize]
    public async Task<TicketResponse> CreateTicket([FromBody] TicketRequestBody ticketBody)
    {
        return await ticketService.CreateTicket(ticketBody);
    }

    [HttpPut("{ticketId}")]
    [Authorize]
    public async Task<IActionResult> UpdateTicket(Guid ticketId, [FromBody] TicketRequestBody ticketBody)
    {
        var updatedTicket =  await ticketService.UpdateTicket(ticketId, ticketBody);
        return Ok(updatedTicket);
    }

    [HttpDelete("{ticketId}")]
    [Authorize]
    public async Task<ActionResult> DeleteTicket([FromQuery] Guid ticketId)
    {
        await ticketService.DeleteTicket(ticketId);
        return NoContent();
    }

    [HttpPost("{ticketId}")]
    [Authorize]
    public async Task<ActionResult> AssignTicketToUser([FromBody] UserTicketAssignDto userDto, Guid ticketId)
    {
        var ticketResponse = await ticketService.AssignTicketToUser(userDto.UserId, ticketId);
        return Ok(ticketResponse);
    }
    
    [HttpGet("hello")]
    public String Hello()
    {
        return "Hello from TicketController .NET!";
    }
}
