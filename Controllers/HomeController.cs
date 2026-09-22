using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        var waitingTokens = _context.ServiceTokens
        .Where(t => t.Status == "Waiting")
        .OrderBy(t => t.CreatedAt)
        .ToList();

        var servingTokens = _context.ServiceTokens
        .Where(t => t.Status == "Serving")
        .OrderBy(t => t.CreatedAt)
        .ToList();

        var model = new HomeViewModel
        {
            WaitingTokens = waitingTokens,
            ServingTokens = servingTokens
        };
        
        return View(model);
    }

    [HttpPost]
    public IActionResult GetToken()
    {
        using var transaction = _context.Database.BeginTransaction();

        var settings = _context.QueueSettings
            .FromSqlRaw(
                "SELECT * FROM QueueSettings WHERE Id = 1 FOR UPDATE")
            .First();

        var waitingCount = _context.ServiceTokens
            .Count(t => t.Status == "Waiting");

        if (waitingCount < settings.MaxWaiting)
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

        transaction.Commit();

        return RedirectToAction("Index");
    }

    [HttpPost]
    public IActionResult ServeNext()
    {
        var settings = _context.QueueSettings.First();

        var servingCount = _context.ServiceTokens
        .Count(t => t.Status == "Serving");

        if (servingCount < settings.MaxServing)
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
        }

        return RedirectToAction("Index");
    }

    [HttpPost]
    public IActionResult CompleteCurrent()
    {
        var token = _context.ServiceTokens
            .Where(t => t.Status == "Serving")
            .OrderBy(t => t.CreatedAt)
            .FirstOrDefault();

        if (token != null)
        {
            token.Status = "Completed";

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
