using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.SignalR;

using SignalRSwaggerGen.Attributes;

namespace api;

[SignalRHub]
[EnableCors(policyName: "AllowFrontend")]
public class GameHub : Hub
{
    private static int _score = 0;

    public override async Task OnConnectedAsync()
    {
        await Clients.Caller.SendAsync("UpdateScore", _score);
    }

    public async Task WhackMole()
    {
        _score++;
        await Clients.All.SendAsync("UpdateScore", _score);
    }
    
    public async Task ResetScore()
    {
        _score = 0;
        await Clients.All.SendAsync("UpdateScore", _score);
    }
}