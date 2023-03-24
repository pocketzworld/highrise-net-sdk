/*

    Copyright (c) 2023 Pocketz World. All rights reserved.

*/

using System.Text.Json.Serialization;

namespace Highrise.API
{
    /// <summary>
    /// Represents an avatar
    /// </summary>
    [Serializable]
    public class Avatar
    {
        /// <summary>
        /// Unique identifier of the user associated with the avatar
        /// </summary>
        [JsonPropertyName("userId")]
        public string UserId { get; set; } = string.Empty;

        /// <summary>
        /// Name of the avatar
        /// </summary>
        [JsonPropertyName("username")]
        public string Name { get; set; } = string.Empty;
    }
}
