/*

    Copyright (c) 2023 Pocketz World. All rights reserved.

*/

using Highrise.API;
using Highrise.API.Modules;
using System.Diagnostics;
using System.Text.Json.Serialization;

var bot = new Bot(
    "wss://highrise.game/web/webapi",
    <ROOM_ID>,
    <API_TOKEN>);

bot.AddModule<TicTacToe>();

try
{
    await bot.Run(async () =>
    {
        // When the bot is connected we will immediately teleport
        await bot.TeleportAsync(3.5f, 0.25f, 4.5f);
    });
}
catch(Exception e)
{
    Console.WriteLine("error: " + e.Message);
}

Console.WriteLine("disconnected");

[System.Serializable]
internal class ChooseMoveEvent : Event<ChooseMoveEvent>
{
    [JsonPropertyName("board_index")]
    public int BoardIndex { get; set;  }
}

[System.Serializable]
internal class GameStartingEvent : Event<GameStartingEvent>
{
    [JsonPropertyName("player_x")]
    public string PlayerX { get; set; } = string.Empty;

    [JsonPropertyName("player_o")]
    public string PlayerO { get; set; } = string.Empty;
}

[System.Serializable]
internal class WaitingForPlayersEvent : Event<WaitingForPlayersEvent>
{
}

[System.Serializable]
internal class GameOverEvent : Event<GameOverEvent>
{
    [JsonPropertyName("winner_id")]
    public string? WinnerId { get; set; }

    [JsonPropertyName("match")]
    public string Match { get; set; } = string.Empty;
}

[RequiresModule(typeof(Storage))]
[RequiresModule(typeof(Leaderboard))]
internal class TicTacToe : Module
{
    private enum State
    {
        Initializing,
        WaitingForPlayers,
        StartingGame,
        Playing,
        GameOver
    }


    private Storage? _storage;
    private Leaderboard? _leaderboard;
    //private int _lastTurn;
    private char[] _board = new char[9];
    private State _state = State.Initializing;
    private string _playerX = string.Empty;
    private string _playerO = string.Empty;
    private Stopwatch _stateTimer = new Stopwatch();

    protected override async Task LoadAsync()
    {
        ChooseMoveEvent.Register();
        GameOverEvent.Register();
        GameStartingEvent.Register();
        WaitingForPlayersEvent.Register();

        _storage = GetModule<Storage>();
        Debug.Assert(_storage != null);

        _leaderboard = GetModule<Leaderboard>();
        Debug.Assert(_leaderboard != null);

        _board = "---------".ToCharArray();
        _storage!.SetString(this, "board", new string(_board));

        _state = State.WaitingForPlayers;
    }

    protected override async Task UpdateAsync()
    {
        switch (_state)
        {
            case State.WaitingForPlayers:
                if (Bot!.UserCount < 1)
                    return;

                _state = State.StartingGame;
                _stateTimer.Restart();

                break;

            case State.StartingGame:
                {
                    if (Bot!.UserCount < 1)
                    {
                        _stateTimer.Stop();
                        _state = State.WaitingForPlayers;
                        return;
                    }

                    if (_stateTimer.IsRunning && _stateTimer.Elapsed.TotalSeconds < 10)
                        return;

                    _stateTimer.Stop();

                    var users = Bot!.Users.ToList();
                    var userIndex0 = Random.Shared.Next() % users.Count;
                    _playerO = users[userIndex0];
                    users.RemoveAt(userIndex0);

                    if (users.Count == 0)
                        _playerX = _playerO;
                    else
                        _playerX = users[Random.Shared.Next() % users.Count];

                    _state = State.Playing;
                    //_lastTurn = 0;

                    await SendLuaEventAsync(new GameStartingEvent { PlayerO = _playerO, PlayerX = _playerX });
                }

                break;

            case State.GameOver:
                if (_stateTimer.Elapsed.TotalSeconds > 5)
                {
                    await ClearBoardAsync();
                    _stateTimer.Restart();
                    _state = State.WaitingForPlayers;
                    await SendLuaEventAsync(new WaitingForPlayersEvent { });
                }
                break;

            case State.Playing:
                // TODO: timeout if no moves are made
                break;
        }
    }

    [EventCallback]
    private async Task OnAvatarJoinedEvent(AvatarJoinedEvent evt)
    {
        if (_state == State.Playing)
            await SendLuaEventAsync(new GameStartingEvent { PlayerO = _playerO, PlayerX = _playerX }, evt.UserId);
    }

    [EventCallback]
    private Task OnChooseMoveEvent(ChooseMoveEvent evt)
    {
#if false
        if (_state != State.Playing)
            return;

        if (evt.BoardIndex < 0 || evt.BoardIndex > 8)
            return;

        if (_board[evt.BoardIndex] != '-')
            return;

        var place = _lastTurn == 0 ? 'x' : 'o';
        if (place == 'x' && evt.SenderId != _playerX)
            return;
        if (place == 'o' && evt.SenderId != _playerO)
            return;

        _lastTurn = (_lastTurn + 1) % 2;

        _board[evt.BoardIndex] = place;

        var winner = EvalulateBoard();
        if (winner != null)
        {
            _state = State.GameOver;
            _stateTimer.Restart();

            var match = "---------".ToCharArray();
            var winnerChar = '-';
            if (winner.Length > 0)
            {
                winnerChar = _board[winner[0]];
                foreach (var i in winner)
                    match[i] = winnerChar;
            }

            var winnerId = "";
            if (winnerChar == 'x')
                winnerId = _playerX;
            else if (winnerChar == 'o')
                winnerId = _playerO;

            if (winnerId != "")
                _leaderboard?.AddScore(winnerId, 1);

            if (winnerChar != '-')
                await Bot!.EmoteAsync("emote-happy");
            else
                await Bot!.EmoteAsync("emoji-crying");

            //if (winnerId != "")
            //{
            //    if (Bot!.TryGetAvatar(winnerId, out var winnerAvatar) && winnerAvatar != null)
            //        await SendChatEventAsync($"{winnerAvatar.Name} wins!");
            //}
            //else
            //    await SendChatEventAsync($"Tie game!");

            await SetBoardAsync(_board);
            await SendLuaEventAsync(new GameOverEvent { Match = new string(match), WinnerId = $"{winnerId}"});

            _state = State.GameOver;
            return;       
        }

        await SetBoardAsync(_board);
#else
        return Task.CompletedTask;
#endif
    }

    private int[]? EvalulateBoard()
    {
        var b = _board;

        // horizontal
        if (b[0] != '-' && b[0] == b[1] && b[0] == b[2]) return new int[] { 0, 1, 2 };
        if (b[3] != '-' && b[3] == b[4] && b[3] == b[5]) return new int[] { 3, 4, 5 };
        if (b[6] != '-' && b[6] == b[7] && b[6] == b[8]) return new int[] { 6, 7, 8 };

        // vertical
        if (b[0] != '-' && b[0] == b[3] && b[0] == b[6]) return new int[] { 0, 3, 6 };
        if (b[1] != '-' && b[1] == b[4] && b[1] == b[7]) return new int[] { 1, 4, 7 };
        if (b[2] != '-' && b[2] == b[5] && b[2] == b[8]) return new int[] { 2, 5, 8 };

        // diagonals
        if (b[0] != '-' && b[0] == b[4] && b[0] == b[8]) return new int[] { 0, 4, 8 };
        if (b[2] != '-' && b[2] == b[4] && b[2] == b[6]) return new int[] { 2, 4, 6 };

        // No winner?
        if (b[0] != '-' && b[1] != '-' && b[2] != '-' &&
            b[3] != '-' && b[4] != '-' && b[5] != '-' &&
            b[6] != '-' && b[7] != '-' && b[8] != '-')
            return new int[0];

        return null;
    }

    private Task ClearBoardAsync() => SetBoardAsync("---------");

    private async Task SetBoardAsync(string board)
    {
        _board = board.ToCharArray();
        await _storage!.SetStringAsync(this, "board", board);
    }

    private Task SetBoardAsync(char[] board) => SetBoardAsync(new string(board));
}



