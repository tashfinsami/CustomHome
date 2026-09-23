using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using CustomHome.Models;
using CustomHome.Services;

namespace CustomHome.Controllers;

public class HomeController : Controller
{
    private readonly QueueService _queueService;

    public HomeController(QueueService queueService)
    {
        _queueService = queueService;
    }

    public IActionResult Index()
    {
        var waitingTokens = _queueService.GetWaitingTokens();
        var servingTokens = _queueService.GetServingTokens();

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
        _queueService.GetToken();
        
        return RedirectToAction("Index");
    }

    [HttpPost]
    public IActionResult ServeNext()
    {
        _queueService.ServeNext();

        return RedirectToAction("Index");
    }

    [HttpPost]
    public IActionResult CompleteCurrent(int id)
    {
        _queueService.Complete(id);

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
