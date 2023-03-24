/*

    Copyright (c) 2023 Pocketz World. All rights reserved.

*/

using Highrise.API;

var bot = new Bot(
    "wss://highrise.game/web/webapi",
    <ROOM_ID>,
    <API_TOKEN>);

bot.AddModule<ParrotModule>();

await bot.Run();

internal class ParrotModule : Module
{
    [EventCallback]
    private async Task OnChatEvent(ChatEvent evt)
    {
        if (!IsSelf(evt.Sender))
            await SendChatRequestAsync(evt.Message);
    }
}


