﻿using BackendTracker.Ticket.NewFolder;
using BackendTrackerDomain.Entity.Ticket;
using BackendTrackerDomain.Interfaces;
using BackendTrackerInfrastructure.Exceptions;
using BackendTrackerInfrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace BackendTrackerInfrastructure.Repositories;

public class TicketRepository(IDbContextFactory<ApplicationContext> contextFactory) : ITicketRepository
{
    public async Task<IEnumerable<Ticket>> GetTickets(Guid submitterId)
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        return await context.Tickets
            .Where(ticket => ticket.SubmitterId == submitterId)
            .ToListAsync();
    }

    public async Task<Ticket?> GetTicketById(Guid ticketId)
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        return await context.Tickets.FirstOrDefaultAsync(m => m.TicketId == ticketId);
    }

    public async Task<Ticket> CreateTicket(
        Ticket ticket)
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        context.Tickets.Add(ticket);
        await context.SaveChangesAsync();
        return ticket;
    }

    public async Task<Ticket> UpdateTicket(
        Ticket ticket)
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        context.Tickets.Update(ticket);
        await context.SaveChangesAsync();
        return ticket;
    }

    public async Task<Ticket> DeleteAsync(Guid ticketId)
    {
        await using var context = await contextFactory.CreateDbContextAsync();

        var ticket = await context.Tickets.FirstOrDefaultAsync(m => m.TicketId == ticketId) ??
                     throw new TicketExceptions("Ticket not found");

        var user = await context.ApplicationUsers.FirstOrDefaultAsync(u =>
                       u.Id == ticket.SubmitterId || u.Id == ticket.AssigneeId)
                   ?? throw new ApplicationUserExceptions("User not found");

        user?.SubmittedTickets.Remove(ticket);

        context.Tickets.Remove(ticket);
        await context.SaveChangesAsync();
        return ticket;
    }

    public async Task<bool> UserExistsAsync(Guid userId)
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        return await context.ApplicationUsers.AnyAsync(u => u.Id == userId);
    }

    public Task<Ticket> AssignTicketToUser(Guid userId, Guid ticketId)
    {
        var context = contextFactory.CreateDbContextAsync().Result;

        var ticket = context.Tickets.FirstOrDefault(t => t.TicketId == ticketId) ??
                     throw new TicketExceptions("Ticket not found");

        var user = context.ApplicationUsers.FirstOrDefault(u => u.Id == userId) ??
                   throw new ApplicationUserExceptions("User not found");

        ticket.AssigneeId = user.Id;
        ticket.Assignee = user;

        context.Tickets.Update(ticket);
        context.SaveChangesAsync();

        return Task.FromResult(ticket);
    }
}