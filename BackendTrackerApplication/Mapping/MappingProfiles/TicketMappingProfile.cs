using AutoMapper;
using BackendTracker.Ticket.NewFolder;
using BackendTrackerApplication.DTOs;
using BackendTrackerDomain.Entity.Ticket;

namespace BackendTrackerApplication.Mapping.MappingProfiles;

public class TicketMappingProfile : Profile
{
   public TicketMappingProfile()
   {
      CreateMap<Ticket, TicketResponse>();
   } 
}