/*

    Copyright (c) 2023 Pocketz World. All rights reserved.

*/

using System.Text.Json.Serialization;

namespace Highrise.API.Events
{
    /// <summary>
    /// Event sent when an avatar leaves the room
    /// </summary>
    [Serializable]
    public class AvatarLeftEvent : Event<AvatarLeftEvent>
    {
        /// <summary>
        /// Identifier of avatar that left the room
        /// </summary>
        [JsonPropertyName("userId")]
        public string UserId { get; set; } = string.Empty;
    }
}
