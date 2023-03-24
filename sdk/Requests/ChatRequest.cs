/*

    Copyright (c) 2023 Pocketz World. All rights reserved.

*/

using System.Text.Json.Serialization;

namespace Highrise.API
{
    /// <summary>
    /// Event used to send chat messages to a user or the entire room
    /// </summary>
    public class ChatRequest : Event<ChatRequest>
    {
        /// <summary>
        /// Chat message recieved or being sent 
        /// </summary>
        [JsonPropertyName("message")]
        public string Message { get; set; }

        /// <summary>
        /// Optional user identifier used to indicate a user to whisper to
        /// </summary>
        [JsonPropertyName("whisper_target_id")]
        public string? WhisperTargetId { get; set; }

        /// <summary>
        /// Construct an empty chat request, this is typically used by json parsing
        /// </summary>
        public ChatRequest() : base("ChatRequest")
        {
            Message = "";
        }
    }
}
