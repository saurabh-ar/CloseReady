using CloseReady.Data;
using CloseReady.Models;
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

    public IActionResult Details(int id)
      {
      var cycle = _context.CollectionCycles.Include(c => c.Client).FirstOrDefault(c => c.Id == id);

      if (cycle == null)
        {
        return NotFound();
        }

      var documents = _context.CycleDocuments.Include(d => d.DocumentType).Where(d => d.CollectionCycleId == id).OrderBy(d => d.DocumentType.Name).ToList();

      ViewBag.Documents = documents;

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

        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(),"wwwroot","uploads");

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

      return RedirectToAction("Details",new { id = document.CollectionCycleId });
      }

    }
  }