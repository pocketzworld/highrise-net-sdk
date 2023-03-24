/*

    Copyright (c) 2023 Pocketz World. All rights reserved.

*/

using System.Text.Json.Serialization;

namespace Highrise.API.Events
{
    /// <summary>
    /// Temporary event used by bot to tell the client they are a bot
    /// </summary>
    [Serializable]
    public class UserJoinedEvent : Event<UserJoinedEvent>
    {
        /// <summary>
        /// True if the user is a bot
        /// </summary>
        [JsonPropertyName("is_bot")]
        public bool IsBot { get; set; }
    }
}
