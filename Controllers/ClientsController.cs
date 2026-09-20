using CloseReady.Data;
using CloseReady.Models;
using Microsoft.AspNetCore.Mvc;

namespace CloseReady.Controllers
  {
  public class ClientsController : Controller
    {
    private readonly ApplicationDbContext _context;

    public ClientsController(ApplicationDbContext context)
      {
      _context = context;
      }

    public IActionResult Index()
      {
      var clients = _context.Clients
          .Where(c => c.IsActive)
          .OrderByDescending(c => c.CreatedAt)
          .ToList();

      return View(clients);
      }
    [HttpGet]
    public IActionResult Create()
      {
      return View();
      }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(Client client)
      {
      if (!ModelState.IsValid)
        {
        return View(client);
        }

      client.CreatedAt = DateTime.UtcNow;
      client.IsActive = true;

      _context.Clients.Add(client);
      _context.SaveChanges();

      return RedirectToAction(nameof(Index));
      }
    public IActionResult Details(int id)
      {
      var client = _context.Clients
          .FirstOrDefault(c => c.Id == id && c.IsActive);

      if (client == null)
        {
        return NotFound();
        }

      return View(client);
      }

    }
  }