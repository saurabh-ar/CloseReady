using CloseReady.Data;
using CloseReady.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Elfie.Serialization;
using Microsoft.EntityFrameworkCore;

namespace CloseReady.Controllers
  {
  public class CollectionCyclesController : Controller
    {
    private readonly ApplicationDbContext _context;

    public CollectionCyclesController(ApplicationDbContext context)
      {
      _context = context;
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

      return RedirectToAction( nameof(ClientUpload),new { token });
      }

    }
  }