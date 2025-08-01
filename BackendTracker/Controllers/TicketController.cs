using BackendTracker.Ticket.NewFolder;
using BackendTracker.Ticket.PayloadAndResponse;
using BackendTrackerApplication.DTOs;
using BackendTrackerApplication.Services;
using BackendTrackerPresentation.Graphql.Subscriptions;
using HotChocolate.Subscriptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackendTrackerPresentation.Controllers;

[ApiController]
[Route("api/tickets")]
public class TicketController(TicketService ticketService) : Controller
{
    [HttpGet]
    [Authorize]
    public async Task<IEnumerable<BackendTrackerDomain.Entity.Ticket.Ticket>> GetTickets([FromQuery] Guid submitterId)
    {
        return await ticketService.GetTickets(submitterId);
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
}