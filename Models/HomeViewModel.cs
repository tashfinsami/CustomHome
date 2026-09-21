namespace CustomHome.Models;

public class HomeViewModel
{
    public List<ServiceToken> WaitingTokens { get; set; }

    public List<ServiceToken> ServingTokens { get; set; }
}