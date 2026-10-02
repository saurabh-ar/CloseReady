using CloseReady.Data;
using CloseReady.Models;
using CloseReady.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Elfie.Serialization;
using Microsoft.EntityFrameworkCore;

namespace CloseReady.Controllers
  {
  public class CollectionCyclesController : Controller
    {
    private readonly ApplicationDbContext _context;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;
    public CollectionCyclesController(ApplicationDbContext context, IEmailService emailService, IConfiguration configuration)
      {
      _context = context;
      _emailService = emailService;
      _configuration = configuration;
      }

    [HttpGet]
    public IActionResult Create(int clientId)
      {
      var client = _context.Clients
          .FirstOrDefault(c => c.Id == clientId && c.IsActive);

      if (client == null)
        {
        return NotFound();
        }

      ViewBag.Client = client;
      ViewBag.DocumentTypes = _context.DocumentTypes.Where(d => d.IsActive).OrderBy(d => d.Name).ToList();

      return View();
      }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(CollectionCycle cycle, int[] SelectedDocumentTypeIds)
      {
      if (!ModelState.IsValid)
        {
        var client = _context.Clients.FirstOrDefault(c => c.Id == cycle.ClientId && c.IsActive);

        ViewBag.Client = client;

        ViewBag.DocumentTypes = _context.DocumentTypes.Where(d => d.IsActive).OrderBy(d => d.Name).ToList();

        return View(cycle);
        }

      cycle.Status = "In Progress";
      cycle.CreatedAt = DateTime.UtcNow;

      _context.CollectionCycles.Add(cycle);
      _context.SaveChanges();

      foreach (var documentTypeId in SelectedDocumentTypeIds)
        {
        var cycleDocument = new CycleDocument
          {
          CollectionCycleId = cycle.Id,
          DocumentTypeId = documentTypeId,
          Status = "Missing",
          CreatedAt = DateTime.UtcNow
          };
        _context.CycleDocuments.Add(cycleDocument);
        }
      _context.SaveChanges();

      return RedirectToAction("Details", "Clients", new { id = cycle.ClientId });
      }

    public IActionResult Details(int? id)
      {
      var cycle = _context.CollectionCycles
          .Include(c => c.Client)
          .FirstOrDefault(c => c.Id == id);

      if (cycle == null)
        {
        return NotFound();
        }

      var documents = _context.CycleDocuments
          .Include(d => d.DocumentType)
          .Where(d => d.CollectionCycleId == id)
          .OrderBy(d => d.DocumentType.Name)
          .ToList();

      var uploadLink = _context.ClientUploadLinks
          .FirstOrDefault(l =>
              l.CollectionCycleId == id &&
              l.IsActive);
      var allDocumentsReceived = documents.Any() && documents.All(d => d.Status == "Received");

      if (allDocumentsReceived && cycle.Status != "Ready")
        {
          cycle.Status = "Ready";
          _context.SaveChanges();
        }

      ViewBag.Documents = documents;
      ViewBag.UploadLink = uploadLink;

      return View("CollectionCyclesDetails", cycle);
      }

    /// <summary>
    /// Upload document
    ///   ↓
    /// Status = Received
    ///   ↓
    /// Are ALL documents Received?
    ///   ↓
    ///   YES
    ///   ↓
    /// Cycle Status = Ready
    /// </summary>

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadDocument(int id, IFormFile file)
      {
      var document = await _context.CycleDocuments.Include(d => d.CollectionCycle).FirstOrDefaultAsync(d => d.Id == id);

      if (document == null)
        {
        return NotFound();
        }

      if (file == null || file.Length == 0)
        {
        return RedirectToAction(
            "Details",
            new { id = document.CollectionCycleId }
        );
        }

      var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");

      Directory.CreateDirectory(uploadsFolder);

      var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
      document.FileName = file.FileName;
      document.StoredFileName = fileName;

      var filePath = Path.Combine(uploadsFolder, fileName);

      using (var stream = new FileStream(filePath, FileMode.Create))
        {
        await file.CopyToAsync(stream);
        }

      document.Status = "Received";

      var cycleDocuments = await _context.CycleDocuments.Where(d => d.CollectionCycleId == document.CollectionCycleId).ToListAsync();

      if (cycleDocuments.All(d => d.Status == "Received"))
        {
        document.CollectionCycle.Status = "Ready";
        }

      await _context.SaveChangesAsync();

      return RedirectToAction("Details", new { id = document.CollectionCycleId });
      }

    /// <summary>
    /// September Close (Cycle name)
    ///   ↓
    /// Create Upload Link
    ///   ↓
    /// Check existing active link
    ///   ↓
    /// No link → create token
    ///   ↓
    /// Save to database
    ///   ↓
    /// Return to September Close
    /// </summary>

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateUploadLink(int cycleId)
      {
      var cycle = await _context.CollectionCycles.FirstOrDefaultAsync(c => c.Id == cycleId);

      if (cycle == null)
        {
        return NotFound();
        }

      var existingLink = await _context.ClientUploadLinks.FirstOrDefaultAsync(l => l.CollectionCycleId == cycleId && l.IsActive);

      if (existingLink == null)
        {
        var uploadLink = new ClientUploadLink
          {
          CollectionCycleId = cycleId,
          Token = Guid.NewGuid().ToString(),
          CreatedAt = DateTime.UtcNow,
          IsActive = true
          };

        _context.ClientUploadLinks.Add(uploadLink);

        await _context.SaveChangesAsync();
        }

      return RedirectToAction("Details", new { id = cycleId });
      }

    [AllowAnonymous]
    [HttpGet("/upload/{token}")]
    public async Task<IActionResult> ClientUpload(string token)
      {
      var uploadLink = await _context.ClientUploadLinks
        .Include(l => l.CollectionCycle)
        .ThenInclude(c => c.Client)
        .Include(l => l.CollectionCycle)
        .ThenInclude(c => c.CycleDocuments)
        .ThenInclude(d => d.DocumentType)
        .FirstOrDefaultAsync(l => l.Token == token && l.IsActive);

      if (uploadLink == null)
        {
        return NotFound();
        }

      return View("CollectionCyclesClientUpload", uploadLink);
      }


    [AllowAnonymous]
    [HttpPost("/upload/{token}/{documentId}")]
    public async Task<IActionResult> ClientUploadFile(string token,int documentId,IFormFile file)
      {
      var uploadLink = await _context.ClientUploadLinks
          .FirstOrDefaultAsync(l =>
              l.Token == token &&
              l.IsActive);

      if (uploadLink == null)
        {
        return NotFound();
        }

      var document = await _context.CycleDocuments
          .FirstOrDefaultAsync(d =>
              d.Id == documentId &&
              d.CollectionCycleId == uploadLink.CollectionCycleId);

      if (document == null)
        {
        return NotFound();
        }

      if (file == null || file.Length == 0)
        {
        return RedirectToAction(nameof(ClientUpload), new { token });
        }

      var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(),"wwwroot","uploads");

      Directory.CreateDirectory(uploadsFolder);

      var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";

      var filePath = Path.Combine(uploadsFolder, fileName);

      using (var stream = new FileStream(filePath, FileMode.Create))
        {
        await file.CopyToAsync(stream);
        }

      document.FileName = file.FileName;
      document.StoredFileName = fileName;
      document.Status = "Received";

      await _context.SaveChangesAsync();

      var allDocumentsReceived = await _context.CycleDocuments
          .Where(d => d.CollectionCycleId == uploadLink.CollectionCycleId)
          .AllAsync(d => d.Status == "Received");

      if (allDocumentsReceived)
        {
        var cycle = await _context.CollectionCycles
            .FirstOrDefaultAsync(c => c.Id == uploadLink.CollectionCycleId);

        if (cycle != null)
          {
          cycle.Status = "Ready";
          await _context.SaveChangesAsync();
          }
        }

      return RedirectToAction( nameof(ClientUpload),new { token });
      }

    /// <summary>
    /// Workflow: Collection Cycle → Upload Link → Client Email
    /// What it does: Sends the active client upload link to the client's email address.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> SendUploadLinkEmail(int cycleId)
      {
          var cycle = await _context.CollectionCycles
              .Include(c => c.Client)
              .FirstOrDefaultAsync(c => c.Id == cycleId);

          if (cycle == null)
            {
            return NotFound();
            }

          if (string.IsNullOrWhiteSpace(cycle.Client?.Email))
            {
            TempData["ErrorMessage"] = "Client email address is missing. Please update the client details first.";

            return RedirectToAction( nameof(Details),  new { id = cycleId });
            }
          var uploadLink = await _context.ClientUploadLinks
                  .FirstOrDefaultAsync(l =>
                      l.CollectionCycleId == cycleId &&
                      l.IsActive);

          if (uploadLink == null)
            {
            return NotFound();
            }

          var appBaseUrl = _configuration["AppBaseUrl"]?? throw new InvalidOperationException("AppBaseUrl is not configured.");

          var clientUploadUrl = $"{appBaseUrl.TrimEnd('/')}/upload/{uploadLink.Token}";

      await _emailService.SendEmailAsync(cycle.Client!.Email,
              $"Documents Required - {cycle.Name}",
              $"""
                  <!DOCTYPE html>
                  <html>
                  <body style="font-family: Arial, sans-serif; color: #333; line-height: 1.6;">

                      <h2 style="color: #222;">Documents Required</h2>

                      <p>Hi {cycle.Client.Name},</p>

                      <p>
                          Please upload the required documents for your
                          <strong>{cycle.Name}</strong> collection cycle.
                      </p>

                      <p>
                          <strong>Due Date:</strong> {cycle.DueDate:M/d/yyyy}
                      </p>

                      <p>
                          <a href="{clientUploadUrl}"
                             style="
                                 display: inline-block;
                                 padding: 12px 20px;
                                 background-color: #212529;
                                 color: white;
                                 text-decoration: none;
                                 border-radius: 5px;
                             ">
                              Upload Documents
                          </a>
                      </p>

                      <p>
                          If the button above does not work, you can use this link:
                      </p>

                      <p>
                          <a href="{clientUploadUrl}">
                              {clientUploadUrl}
                          </a>
                      </p>

                      <p>
                          Thank you,<br />
                          <strong>CloseReady</strong>
                      </p>

                  </body>
                  </html>
                  """);
      TempData["SuccessMessage"] =  $"Upload link emailed successfully to {cycle.Client.Email}.";
      return RedirectToAction(nameof(Details), new { id = cycleId });
      }

    }
  }