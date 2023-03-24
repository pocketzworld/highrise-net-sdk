/*

    Copyright (c) 2023 Pocketz World. All rights reserved.

*/

using System.Text.Json.Serialization;

namespace Highrise.API
{
    /// <summary>
    /// Event used to send or receive chat messages to a user or the entire room
    /// </summary>
    public class ChatEvent : Event<ChatEvent>
    {
        /// <summary>
        /// User identifier that sent the message
        /// </summary>
        [JsonPropertyName("whisper")]
        public bool IsWhisper { get; set; } = false;

        /// <summary>
        /// Chat message recieved or being sent 
        /// </summary>
        [JsonPropertyName("message")]
        public string Message { get; set; }

        /// <summary>
        /// Construct an empty chat event, this is typically used by json parsing
        /// </summary>
        public ChatEvent() : base("ChatEvent")
        {
            Message = "";
        }
    }
}
