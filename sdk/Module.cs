/*

    Copyright (c) 2023 Pocketz World. All rights reserved.

*/

using Microsoft.Extensions.Configuration;

namespace Highrise.API
{
    /// <summary>
    /// Defines the interface to a bot module
    /// </summary>
    public abstract class Module
    {
        internal IConfigurationSection? _configuration;

        /// <summary>
        /// Bot the module is loading on
        /// </summary>
        public Bot? Bot { get; internal set; }
    
        /// <summary>
        /// Implement to handle the asynchronous loading of a module
        /// </summary>
        protected internal virtual Task LoadAsync() => Task.CompletedTask;

        /// <summary>
        /// Implement to handle the asynchronous unloading of a module
        /// </summary>
        protected internal virtual Task UnloadAsync() => Task.CompletedTask;

        /// <summary>
        /// Update a single frame
        /// </summary>
        protected internal virtual Task UpdateAsync() => Task.CompletedTask;

        /// <summary>
        /// Returns true if the user id matches the bot's user id
        /// </summary>
        protected bool IsSelf(User? user) => user != null && Bot!.IsSelf(user);

        /// <summary>
        /// Return the configuration string that matches the given key name
        /// </summary>
        protected string GetConfigString(string key, string defaultValue="") =>
            _configuration?.GetValue<string>(key) ?? defaultValue;

        /// <summary>
        /// Return the configuration string that matches the given key name
        /// </summary>
        protected string[] GetConfigStrings(string key) =>
            _configuration
                ?.GetSection(key)
                ?.GetChildren()
                .Select(c => c.Get<string>() ?? string.Empty)
                .ToArray() ?? new string[0];

        /// <summary>
        /// Send an event asynchronously to the bot
        /// </summary>
        /// <param name="evt"></param>
        private Task SendEventAsync(Event evt) =>
            Bot!.SendEventAsync(evt);

        /// <summary>
        /// Send a chat message 
        /// </summary>
        protected Task SendChatRequestAsync(string message, string? whisperTargetId = null) =>
            SendEventAsync(new ChatRequest { Message = message, WhisperTargetId = whisperTargetId });

        /// <summary>
        /// Send a channel request
        /// </summary>
        protected Task SendChannelRequestAsync(string message, string? tags = null) =>
            SendEventAsync(new ChannelRequest { Message = message, Tags = tags ?? "" });

        /// <summary>
        /// Send a module request 
        /// </summary>
        protected Task SendModuleEventAsync(Event evt) =>
            Bot!.SendModuleEventAsync(this, evt);

        /// <summary>
        /// Send a module request 
        /// </summary>
        protected Task SendModuleEventAsync(Event evt, string targetId) =>
            Bot!.SendModuleEventAsync(this, evt, targetId: targetId);

        /// <summary>
        /// Send a lua event all users in the room.
        /// </summary>
        protected Task SendLuaEventAsync(Event evt) =>
            Bot!.SendModuleEventAsync(this, evt);

        /// <summary>
        /// Send a lua event to a specific user in the room
        /// </summary>
        protected Task SendLuaEventAsync(Event evt, string targetId) =>
            Bot!.SendModuleEventAsync(this, evt, targetId);

        /// <summary>
        /// Returns the module of the given type that is registered with the parent bot
        /// </summary>
        protected TModule? GetModule<TModule>() where TModule : Module =>
            Bot!.GetModule<TModule>();
    }
}
