namespace CustomHome.Models;

public class HomeViewModel
{
    public List<ServiceToken> WaitingTokens { get; set; }

    public ServiceToken? ServingToken { get; set; }
}