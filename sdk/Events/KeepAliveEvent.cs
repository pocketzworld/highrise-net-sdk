/*

    Copyright (c) 2023 Pocketz World. All rights reserved.

*/

namespace Highrise.API.Events
{
    /// <summary>
    /// Event sent to ensure the bot is not disconnected from the server
    /// </summary>
    internal class KeepAliveEvent : Event<KeepAliveEvent>
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public KeepAliveEvent() : base("keepalive")
        {
        }
    }
}
