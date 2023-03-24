/*

    Copyright (c) 2023 Pocketz World. All rights reserved.

*/

using System.Text.Json.Serialization;
using Highrise.API.Models;

namespace Highrise.API
{
    /// <summary>
    /// Event that is sent to teleport the bot or another user to a specific location
    /// </summary>
    [Serializable]
    internal class TeleportEvent : Event<TeleportEvent>
    {
        /// <summary>
        /// Desination location to teleport to
        /// </summary>
        [JsonIgnore]
        public Vector3 Destination { get; set; }

        /// <summary>
        /// Identifier of the user to teleport
        /// </summary>
        [JsonPropertyName("user_id")]
        public string UserId { get; set; } = string.Empty;

        /// <summary>
        /// Represents the serialized value for the destination
        /// </summary>
        [JsonPropertyName("destination")]
        public float[] SerializedDestination
        {
            get => new float[] { Destination.x, Destination.y, Destination.z };
            set => Destination = new Vector3(value[0], value[1], value[2]);
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        public TeleportEvent() : base("teleport")
        {
        }
    }
}
