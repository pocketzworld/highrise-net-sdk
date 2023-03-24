/*

    Copyright (c) 2023 Pocketz World. All rights reserved.

*/

using System.Text.Json.Serialization;

namespace Highrise.API.Events
{
    /// <summary>
    /// User has left the room
    /// </summary>
    [Serializable]
    public class UserLeftEvent : Event<UserLeftEvent>
    {
        /// <summary>
        /// True if the user is a bot
        /// </summary>
        [JsonPropertyName("is_bot")]
        public bool IsBot { get; set; }
    }
}
