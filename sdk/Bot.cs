/*

    Copyright (c) 2023 Pocketz World. All rights reserved.

*/

using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net.WebSockets;
using System.Reflection;
using System.Text.Json;
using Highrise.API.Events;
using Highrise.API.Models;
using Microsoft.Extensions.Configuration;

namespace Highrise.API
{
    /// <summary>
    /// Defines a Highrise bot that can connect to a room in highrise
    /// </summary>
    public class Bot
    {
        private class CallbackFunctor
        {
            public Module _module;
            public MethodInfo _methodInfo;
            public Type _eventType;
            public bool _async;

            public CallbackFunctor(Module module, Type eventType, MethodInfo methodInfo)
            {
                _module = module;
                _methodInfo = methodInfo;
                _eventType = eventType;
                _async = _methodInfo.ReturnType == typeof(Task);
            }

            public async Task InvokeAsync(Event message)
            {
                Debug.Assert(message.GetType() == _eventType);

                if (_async)
                {
                    var result = _methodInfo.Invoke(_module, new object[] { message });
                    if (result is Task task)
                        await task;
                }                    
                else
                    _methodInfo.Invoke(_module, new object[] { message });
            }

            public bool IsEquivalentTo(CallbackFunctor functor) =>
                _module == functor._module && 
                functor._methodInfo == _methodInfo && 
                _eventType == functor._eventType;
        }

        private ConcurrentQueue<Event> _messages = new();
        private ClientWebSocket? _socket = null;
        private CancellationTokenSource? _cancellationTokenSource;
        private Task? _receiveTask;
        private List<Module> _modules = new();
        private Dictionary<Type, List<CallbackFunctor>> _callbacks = new();
        private Stopwatch _keepAliveTimer = Stopwatch.StartNew();
        private Dictionary<string,Avatar> _users = new();
        private User _user;
        private IConfiguration _configuration;

        /// <summary>^
        /// User identifier of the bot in the room
        /// </summary>
        public string UserId => _user.Id;

        /// <summary>
        /// Return all connected users
        /// </summary>
        public IEnumerable<string> Users => _users.Select(u => u.Value.UserId);

        /// <summary>
        /// Return the number of connected users
        /// </summary>
        public int UserCount => _users.Count;

        /// <summary>
        /// Returns true if the bot is still connected to a room
        /// </summary>
        public bool IsConnected => _socket != null && _socket.State == WebSocketState.Open;

        /// <summary>
        /// URL the bot should connect to
        /// </summary>
        public string Url { get; }

        /// <summary>
        /// API token used to authorize the bot
        /// </summary>
        public string ApiToken { get; }

        /// <summary>
        /// Identifier of room the bot should connect to
        /// </summary>
        public string RoomId { get; }

        /// <summary>
        /// Construct a new bot
        /// </summary>
        public Bot()
        {
            _configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("botsettings.json")
                .Build();

            Url = _configuration.GetValue<string>("api_url", "")!;
            ApiToken = _configuration.GetValue<string>("api_key", "")!;
            RoomId = _configuration.GetValue<string>("room_id", "")!;

            _user = new User("", "");

            // Force registration of all supported events to ensure they are linked
            SessionMetadata.Register();
            ChatEvent.Register();
            AvatarJoinedEvent.Register();
            AvatarLeftEvent.Register();
            UserJoinedEvent.Register();
            UserLeftEvent.Register();
            KeepAliveEvent.Register();
            TeleportEvent.Register();
            EmoteEvent.Register();
            ChatRequest.Register();
            KeepaliveRequest.Register();
            ChannelEvent.Register();
            ChannelRequest.Register();
        }

        /// <summary>
        /// Connect the bot to a room
        /// </summary>
        private async Task<bool> ConnectAsync()
        {
            _socket = new ClientWebSocket();
            _socket.Options.SetRequestHeader("room-id", RoomId);
            _socket.Options.SetRequestHeader("api-token", ApiToken);

            try
            {
                await _socket.ConnectAsync(new Uri(Url), CancellationToken.None);
            }
            catch(Exception e)
            {
                Console.WriteLine($"error: {e.Message}");

                return false;
            }

            if (_socket.State != WebSocketState.Open)
                return false;

            _cancellationTokenSource = new CancellationTokenSource();
            _receiveTask = Task.Run(ReceiveThread, _cancellationTokenSource.Token);

            // Let all the users know there is a new bot in town
            //await SendModuleEventAsync(null, new UserJoinedEvent { IsBot = true });

            return true;
        }

        /// <summary>
        /// Disconnect the bot from the room
        /// </summary>
        public async Task DisconnectAsync(CancellationToken token) 
        {
            if (_socket == null)
                return;

            _cancellationTokenSource?.Cancel();

            await _socket.CloseAsync(WebSocketCloseStatus.NormalClosure, null, token);

            if (_receiveTask != null)
                await _receiveTask.WaitAsync(token);

            _cancellationTokenSource = null;
        }

        /// <summary>
        /// Try to return the avatar for the given user identifier.
        /// </summary>
        public bool TryGetAvatar(string id, out Avatar? avatar) =>
            _users.TryGetValue(id, out avatar);

        /// <summary>
        /// Try to get the next incoming message
        /// </summary>
        public bool TryGetMessage(out Event? message)
        {
            // Wait for session meta data
            if (UserId == null)
            {
                message = null;
                return false;
            }

            return _messages.TryDequeue(out message);
        }
            
        /// <summary>
        /// Send a message to the server
        /// </summary>
        public async Task SendEventAsync(Event evt)
        {
            if (_socket == null || _socket.State != WebSocketState.Open)
                return;

            evt.Sender = _user;

            var sendString = JsonSerializer.Serialize(evt, evt.GetType());
            var sendBytes = System.Text.Encoding.UTF8.GetBytes(sendString);

            await _socket.SendAsync(sendBytes, WebSocketMessageType.Text, true, _cancellationTokenSource?.Token ?? CancellationToken.None);
        }

        internal async Task SendModuleEventAsync(Module module, Event evt, string? targetId=null)
        {
            if (_socket == null || _socket.State != WebSocketState.Open)
                return;

            // Add in the module name
            evt.Module = module.GetType().Name;

            // Encode the message
            var encodedMessage = JsonSerializer.Serialize(evt, evt.GetType());
            var tags = "ModuleEvent";

            await SendEventAsync(new ChannelRequest
            {
                Message = encodedMessage,
                Tags = tags
            });
        }

        private async Task ReceiveThread()
        {
            try
            {
                if (_cancellationTokenSource == null)
                    return;

                var buffer = new ArraySegment<byte>(new byte[8192]);
                var combined = new MemoryStream(8192);

                while (_socket != null && _socket.State == WebSocketState.Open && !_cancellationTokenSource.IsCancellationRequested)
                {
                    WebSocketReceiveResult? result = default;

                    // Reset the memory stream
                    combined.Position = 0;
                    combined.SetLength(0);

                    do
                    {
                        result = await _socket.ReceiveAsync(buffer, _cancellationTokenSource.Token);
                        combined.Write(buffer.Array!, buffer.Offset, result.Count);
                    }
                    while (!result.EndOfMessage && !_cancellationTokenSource.IsCancellationRequested);

                    if (result.EndOfMessage)
                    {
                        var messageJson = System.Text.Encoding.UTF8.GetString(combined.ToArray());

                        try
                        {
                            var message = Event.Parse(messageJson);

                            if (message != null)
                            {
                                if (message is SessionMetadata metaData)
                                    _user = new User(metaData.UserId, "");

                                _messages.Enqueue(message);
                            }
                        }
                        catch
                        {
                        }

                        Console.WriteLine("recv: " + messageJson);
                    }
                }
            } 
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        /// <summary>
        /// Run the bot until disconnected 
        /// </summary>
        public async Task Run(Func<Task>? startup=null) 
        {
            if (!await ConnectAsync())
                return;

            foreach (var module in _modules)
            {
                
                await module.LoadAsync();
            }
                

            if (startup != null)
                await startup();

            while (IsConnected)
            {
                // Slow the bot down, it doesnt need to run full tilt
                await Task.Delay(100);

                // Send keep alive
                if (_keepAliveTimer.ElapsedMilliseconds > 15000)
                {
                    await SendEventAsync(new KeepaliveRequest());
                    _keepAliveTimer.Restart();
                }

                if (TryGetMessage(out var message) && message != null)
                    await InvokeCallbacksAsync(message);

                // Give each module a chance to update as well
                foreach (var module in _modules)
                    await module.UpdateAsync();
            }

            // Unload in reverse order
            for (int i=_modules.Count - 1 ;i >= 0; i--)
                await _modules[i].UnloadAsync();
        }

        /// <summary>
        /// Add a new module to the bot of a given type
        /// </summary>
        public Bot AddModule<TModule>() where TModule : Module, new()
        {
            AddModule(typeof(TModule));
            return this;
        }            

        /// <summary>
        /// Add a new module to the bot of the given type
        /// </summary>
        public Module? AddModule(Type moduleType)
        {
            // Module requirements?
            foreach (var require in moduleType.GetCustomAttributes<RequiresModuleAttribute>())
                AddModule(require.ModuleType);

            var module = GetModule(moduleType);
            if (module != null)
                return module;

            module = (Module?)Activator.CreateInstance(moduleType);
            if (null == module)
                throw new System.InvalidOperationException($"Could not create module of type '{moduleType.Name}'");

            module.Bot = this;
            module._configuration = _configuration.GetSection("modules").GetSection(moduleType.Name);
            AddCallbacks(module);
            _modules.Add(module);
            return module;
        }

        private void ThrowInvalidEventCallback(MethodInfo method) =>
            throw new InvalidOperationException($"Invalid signature for event callback: {(method.DeclaringType?.FullName + "." ?? "") + method.Name}");

        private void AddCallbacks(Module module)
        {
            var methods = module.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (var method in methods.Where(m => m.GetCustomAttribute<EventCallbackAttribute>() != null))
            {
                if (method.ReturnType != null && method.ReturnType.Name != "Void" && method.ReturnType != typeof(Task))
                    ThrowInvalidEventCallback(method);

                var parameters = method.GetParameters();
                if (parameters.Length != 1 || !parameters[0].ParameterType.IsAssignableTo(typeof(Event)))
                    ThrowInvalidEventCallback(method);

                var eventType = parameters[0].ParameterType;
                if (!_callbacks.TryGetValue(eventType, out var functors))
                {
                    functors = new List<CallbackFunctor>();
                    _callbacks[eventType] = functors;

                    // Register the event type
                    eventType.GetMethod("Register", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)!.Invoke(null, null);
                }

                functors.Add(new CallbackFunctor(module, eventType, method));
            }
        }

        private string DecodeEvent(string evt) => evt.Replace('@', '\"'); //  HttpUtility.UrlDecode(evt);
        private string EncodeEvent(string evt) => evt.Replace('\"', '@'); //  HttpUtility.UrlEncode(evt);

        private void ProcessCustomEvents(ChannelEvent evt)
        {
            // Custom events should have the "CustomEvent" tag.
            if (!evt.Tags.Contains("CustomEvent"))
                return;

            // Custom events are just json so parse them and route them.

        }

        private async Task InvokeCallbacksAsync(Event evt)
        {
            Module? module = null;

            if (evt is ChannelEvent channelEvent)
                ProcessCustomEvents(channelEvent);
            





            if (evt is ChatEvent chatEvent)
            {
#if false
                // Module event?
                if (chatEvent.Message.StartsWith(LuaEventControlChar))
                {
                    // Ignore any messages we originally sent out
                    if (chatEvent.UserId == UserId)
                        return;

                    var split = chatEvent.Message.Split(LuaEventControlChar);
                    if (split.Length != 3)
                    {
                        Console.WriteLine("error: invalid module event: " + chatEvent.Message);
                        return;
                    }

                    var moduleName = split[1];
                    var moduleEventJson = DecodeEvent(split[2]);
                    var moduleEvent = Event.Parse(moduleEventJson);
                    if (moduleEvent is null)
                        return;

                    moduleEvent.SenderId = chatEvent.UserId;
                    module = _modules.FirstOrDefault(m => m.GetType().Name == moduleName);
                    evt = moduleEvent;
                }
#endif
            }
            else if (evt is AvatarLeftEvent leftEvent)
            {
                _users.Remove(leftEvent.UserId);
            }
            else if (evt is AvatarJoinedEvent joinedEvent)
            {
                if (joinedEvent.UserId != UserId)
                {
                    _users.Add(joinedEvent.UserId, joinedEvent.Avatar!);
                    //await SendModuleEventAsync(null, new UserJoinedEvent { IsBot = true }, evt.SenderId);
                }

            }

            if (!_callbacks.TryGetValue(evt.GetType(), out var functors))
                return;

            foreach (var functor in functors)
                if (module == null || module == functor._module)
                    await functor.InvokeAsync(evt);
        }

        /// <summary>
        /// Returns the module of the given type
        /// </summary>
        internal TModule? GetModule<TModule>() where TModule : Module =>
            _modules.FirstOrDefault(m => m.GetType().IsAssignableTo(typeof(TModule))) as TModule;

        /// <summary>
        /// Returns the module of the given type
        /// </summary>
        internal Module? GetModule(Type moduleType) =>
            _modules.FirstOrDefault(m => m.GetType().IsAssignableTo(moduleType));

        /// <summary>
        /// Returns true if the given user identifier is the bot's identifier
        /// </summary>
        public bool IsSelf(User user) => UserId == user.Id;

        /// <summary>
        /// Teleport the bot to a location
        /// </summary>
        public Task TeleportAsync(float x, float y, float z) =>
            SendEventAsync(new TeleportEvent { UserId = UserId, Destination = new Vector3(x,y,z)});

        /// <summary>
        /// Play an emote on the bot
        /// </summary>
        public Task EmoteAsync(string emote) =>
            SendEventAsync(new EmoteEvent { Emote = emote });
    }
}
