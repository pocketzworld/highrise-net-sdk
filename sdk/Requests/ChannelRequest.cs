/*

    Copyright (c) 2023 Pocketz World. All rights reserved.

*/

using System.Text.Json.Serialization;
using System.Xml.Serialization;

namespace Highrise.API
{
    /// <summary>
    /// Request to send a hidden channel message to a channel
    /// </summary>
    public class ChannelRequest : Event<ChannelRequest>
    {
        /// <summary>
        /// Chat message recieved or being sent 
        /// </summary>
        [JsonPropertyName("message")]
        //[XmlElement(DataType = "CDATA")]
        public string Message { get; set; }

        /// <summary>
        /// Construct an empty chat request, this is typically used by json parsing
        /// </summary>
        public ChannelRequest()
        {
            Message = "";
        }
    }
}
