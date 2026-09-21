using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using CustomHome.Models;

namespace CustomHome.Controllers;

public class HomeController : Controller
{
    private static Queue<ServiceToken> tokens = new Queue<ServiceToken>();

    public IActionResult Index()
    {
        return View(tokens);
    }

    [HttpPost]
    public IActionResult GetToken()
    {
        if (tokens.Count < 5)
        {
            tokens.Enqueue(new ServiceToken
            {
                TokenNumber = Random.Shared.Next(100000, 999999),
                Status = "Waiting"
            });
        }

        return RedirectToAction("Index");
    }

    [HttpPost]
    public IActionResult ServeNext()
    {
        if (tokens.Count > 0)
        {
            tokens.Dequeue();
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
