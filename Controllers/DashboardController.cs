using CloseReady.Data;
using CloseReady.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CloseReady.Controllers
  {
  public class DashboardController : Controller
    {
    private readonly ApplicationDbContext _context;
    private readonly IEmailService _emailService;

    public DashboardController(ApplicationDbContext context, IEmailService emailService)
      {
      _context = context;
      _emailService = emailService;
      }

    /// <summary>
    /// Workflow: Dashboard → Collection Overview
    /// What it does: Displays the main CloseReady dashboard.
    /// </summary>
    public IActionResult Index()
      {
      var totalClients = _context.Clients
          .Count(c => c.IsActive);

      var activeCycles = _context.CollectionCycles
          .Count();

      var inProgressCycles = _context.CollectionCycles
          .Count(c => c.Status == "In Progress");

      var readyCycles = _context.CollectionCycles
          .Count(c => c.Status == "Ready");

      ViewBag.TotalClients = totalClients;
      ViewBag.ActiveCycles = activeCycles;
      ViewBag.InProgressCycles = inProgressCycles;
      ViewBag.ReadyCycles = readyCycles;

      return View("DashboardIndex");
      }
    /// <summary>
    /// Workflow: Dashboard → In Progress → Collection Cycles
    /// What it does: Shows all collection cycles that are still in progress.
    /// </summary>
    public IActionResult InProgress()
      {
      //Why: Client is a navigation property. We need .Include(c => c.Client) so the dashboard has the client information available to display.
      var cycles = _context.CollectionCycles.Include(c => c.Client).Where(c => c.Status == "In Progress").OrderBy(c => c.DueDate).ToList();

      return View("DashboardInProgress", cycles);
      }

    /// <summary>
    /// Workflow: Dashboard → Missing Documents → Identify outstanding client documents
    /// What it does: Displays all documents that have not yet been received for in-progress cycles.
    /// </summary>
    public IActionResult MissingDocuments()
      {
      var missingDocuments = _context.CycleDocuments
          .Include(d => d.CollectionCycle)
              .ThenInclude(c => c.Client)
          .Include(d => d.DocumentType)
          .Where(d =>
              d.Status == "Missing" &&
              d.CollectionCycle.Status == "In Progress")
          .OrderBy(d => d.CollectionCycle.DueDate)
          .ToList();

      return View("DashboardMissingDocuments", missingDocuments);
      }

    /// <summary>
    /// Workflow: Dashboard → Email Service → Gmail SMTP → Test Recipient
    /// What it does: Sends a test email to verify Gmail SMTP configuration.
    /// replace the gmail with your gmail.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> SendTestEmail()
      {
      await _emailService.SendEmailAsync("kavyanshnaiducr@gmail.com", "[URGENT] App Email Test", "This is a test email from CloseReady.");

      return Content("Test email sent successfully.");
      }

    }
  }