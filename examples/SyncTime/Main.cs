/*

    Copyright (c) 2023 Pocketz World. All rights reserved.

*/

using Highrise.API;
using Highrise.API.Modules;
using System.Text.Json.Serialization;

SyncTimeEvent.Register();

var bot = new Bot(
    "wss://highrise.game/web/webapi",
    <ROOM_ID>,
    <API_TOKEN>);

bot.AddModule<SyncTime>();

await bot.Run();

[System.Serializable]
internal class SyncTimeEvent : Event<SyncTimeEvent>
{
    [JsonPropertyName("time")]
    public long Time { get; set;  }
}

[RequiresModule(typeof(Storage))]
internal class SyncTime : Module
{
    private static readonly DateTime _baseline = new DateTime(2000, 1, 1);
    private System.Diagnostics.Stopwatch _stopwatch = new();
    private long _time;
    private Storage? _storage;

    protected override async Task LoadAsync()
    {
        _storage = GetModule<Storage>();

        await UpdateTime();
    }

    protected override async Task UpdateAsync()
    {
        if (_stopwatch.Elapsed.TotalSeconds < 60)
            return;

        await UpdateTime();
    }

    private async Task UpdateTime()
    {
        _time = (DateTime.Now - _baseline).Ticks / 10000;
        await SendLuaEventAsync(new SyncTimeEvent { Time = _time });
        _stopwatch.Restart();

        await _storage!.SetStringAsync(this, "time", _time.ToString());
    }

    [EventCallback]
    private async Task OnAvatarJoined(AvatarJoinedEvent evt)
    {
        await SendLuaEventAsync(new SyncTimeEvent { Time = _time }, evt.UserId);
    }
}



