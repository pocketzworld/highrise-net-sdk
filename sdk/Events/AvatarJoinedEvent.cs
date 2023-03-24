/*

    Copyright (c) 2023 Pocketz World. All rights reserved.

*/

using System.Text.Json.Serialization;

namespace Highrise.API
{
    /// <summary>
    /// Event sent by server when an avatar joins the room
    /// </summary>
    public class AvatarJoinedEvent : Event<AvatarJoinedEvent>
    {
        /// <summary>
        /// Avatar that joined
        /// </summary>
        [JsonPropertyName("avatar")]
        public Avatar? Avatar { get; set; }

        /// <summary>
        /// Name of the user that owns the avatar
        /// </summary>
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// Identifier of the user that owns the avatar
        /// </summary>
        [JsonPropertyName("userId")]
        public string UserId { get; set; } = string.Empty;
    }
}
