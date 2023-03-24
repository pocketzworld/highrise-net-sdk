/*

    Copyright (c) 2023 Pocketz World. All rights reserved.

*/

using System.Text.Json.Serialization;

namespace Highrise.API
{
    /// <summary>
    /// Sent by the server when the bot is first connected
    /// </summary>
    [Serializable]
    internal class SessionMetadata : Event<SessionMetadata>
    {
        /// <summary>
        /// User identifier of the bot
        /// </summary>
        [JsonPropertyName("user_id")]
        public string UserId { get; set; }

        /// <summary>
        /// Default constructor for JSON parsing
        /// </summary>
        public SessionMetadata() : base("SessionMetadata")
        {
            UserId = "";
        }
    }
}
