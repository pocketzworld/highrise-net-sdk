/*

    Copyright (c) 2023 Pocketz World. All rights reserved.

*/

using System.Text.Json.Serialization;

namespace Highrise.API
{
    /// <summary>
    /// Defines a user in the system
    /// </summary>
    [Serializable]
    public class User
    {
        /// <summary>
        /// Identifier of the user
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; } = "";

        /// <summary>
        /// Name of the user
        /// </summary>
        [JsonPropertyName("username")]
        public string Username { get; set; } = "";

        /// <summary>
        /// Construct a default user
        /// </summary>
        public User()
        {
        }

        /// <summary>
        /// Construct a user from an identifier and name
        /// </summary>
        /// <param name="id"></param>
        /// <param name="username"></param>
        public User(string id, string username)
        {
            Id = id;
            Username = username;
        }
    }
}
