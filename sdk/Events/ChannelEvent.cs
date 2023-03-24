/*

    Copyright (c) 2023 Pocketz World. All rights reserved.

*/

using System.Text.Json.Serialization;

namespace Highrise.API
{
    /// <summary>
    /// Event received when a channel event has been sent to the room
    /// </summary>
    public class ChannelEvent : Event<ChannelEvent>
    {
        /// <summary>
        /// Message sent
        /// </summary>
        [JsonPropertyName("msg")]
        public string Message { get; set; }

        /// <summary>
        /// Identifier of the sender who sent the message
        /// </summary>
        [JsonPropertyName("sender_id")]
        public string SenderId { get; set; }

        /// <summary>
        /// Construct an empty channel event
        /// </summary>
        public ChannelEvent()
        {
            Message = "";
            Tags = "";
            SenderId = "";
        }
    }
}
