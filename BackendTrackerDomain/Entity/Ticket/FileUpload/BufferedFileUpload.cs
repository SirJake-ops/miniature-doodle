using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BackendTrackerDomain.Entity.Ticket.FileUpload;

public class BufferedFileUpload : PageModel
{
    [BindProperty] public BufferedFileUploadDb? FileUpload { get; set; }
}

public class BufferedFileUploadDb
{
    [Required] [Display(Name = "file")] public IFormFile? File { get; set; }
}