using CloseReady.Data;
using CloseReady.Models;
using Microsoft.AspNetCore.Mvc;

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

      return View();
      }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(CollectionCycle cycle)
      {
      if (!ModelState.IsValid)
        {
        var client = _context.Clients
            .FirstOrDefault(c => c.Id == cycle.ClientId && c.IsActive);

        ViewBag.Client = client;

        return View(cycle);
        }

      cycle.Status = "In Progress";
      cycle.CreatedAt = DateTime.UtcNow;

      _context.CollectionCycles.Add(cycle);
      _context.SaveChanges();

      return RedirectToAction(
          "Details",
          "Clients",
          new { id = cycle.ClientId }
      );
      }
    }
  }