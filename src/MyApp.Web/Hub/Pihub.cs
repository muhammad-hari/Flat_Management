using Microsoft.AspNetCore.SignalR;

public class PiHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        var piId = Context.GetHttpContext().Request.Query["piId"].ToString();
        if(!string.IsNullOrEmpty(piId))
            await Groups.AddToGroupAsync(Context.ConnectionId, piId);
        await base.OnConnectedAsync();
    }

    // Pi -> server: ReceiveTap
    public async Task ReceiveTap(object tap)
    {
        // tap is dynamic (uid, reader_id, timestamp)
        // Simpan ke DB, cek akses, dsb.
        Console.WriteLine("ReceiveTap: " + tap.ToString());
        // contoh: broadcast ke admin clients if needed
        await Clients.Group("admin").SendAsync("NewTap", tap);
    }

    // Server -> Pi: SendCommand (unlock)
    public async Task SendCommandToPi(string piId, object command)
    {
        await Clients.Group(piId).SendAsync("ReceiveCommand", command);
    }

    // Pi -> server: RelayStatus
    public async Task RelayStatus(object status)
    {
        Console.WriteLine("RelayStatus: " + status.ToString());
    }
}
