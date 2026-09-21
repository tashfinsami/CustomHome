using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using CustomHome.Data;
using CustomHome.Models;

namespace CustomHome.Controllers;

public class HomeController : Controller
{
    private readonly ServiceStationContext _context;

    public HomeController(ServiceStationContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        var tokens = _context.ServiceTokens
        .Where(t => t.Status == "Waiting")
        .OrderBy(t => t.CreatedAt)
        .ToList();

        var waitingTokens = _context.ServiceTokens
        .Where(t => t.Status == "Waiting")
        .OrderBy(t => t.CreatedAt)
        .ToList();

        var servingToken = _context.ServiceTokens
            .FirstOrDefault(t => t.Status == "Serving");

        var model = new HomeViewModel
        {
            WaitingTokens = waitingTokens,
            ServingToken = servingToken
        };

    return View(model);

        return View(tokens);
    }

    [HttpPost]
    public IActionResult GetToken()
    {
        var waitingCount = _context.ServiceTokens
        .Count(t => t.Status == "Waiting");

        if (waitingCount < 5)
        {
            var token = new ServiceToken
            {
                TokenNumber = Random.Shared.Next(100000, 999999),
                Status = "Waiting",
                CreatedAt = DateTime.Now
            };

            _context.ServiceTokens.Add(token);
            _context.SaveChanges();
        }

        return RedirectToAction("Index");
    }

    [HttpPost]
    public IActionResult ServeNext()
    {
        var token = _context.ServiceTokens
        .Where(t => t.Status == "Waiting")
        .OrderBy(t => t.CreatedAt)
        .FirstOrDefault();

        if (token != null)
        {
            token.Status = "Serving";

            _context.SaveChanges();
        }

        return RedirectToAction("Index");
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
