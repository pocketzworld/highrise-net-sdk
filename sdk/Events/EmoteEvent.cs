/*

    Copyright (c) 2023 Pocketz World. All rights reserved.

*/

using System.Text.Json.Serialization;

namespace Highrise.API.Events
{
    /// <summary>
    /// Event sent to play an emote
    /// </summary>
    [Serializable]
    internal class EmoteEvent : Event<EmoteEvent>
    {
        /// <summary>
        /// Identifier of the emote to play
        /// </summary>
        [JsonPropertyName("emote_id")]
        public string Emote { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public EmoteEvent() : base("emote")
        {
            Emote = "";
        }
    }
}
