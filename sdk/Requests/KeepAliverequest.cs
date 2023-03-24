/*

    Copyright (c) 2023 Pocketz World. All rights reserved.

*/

namespace Highrise.API
{
    /// <summary>
    /// Request used to keep the bot from being disconnected
    /// </summary>
    public class KeepaliveRequest : Event<KeepaliveRequest>
    {
        /// <summary>
        /// Construct an empty request
        /// </summary>
        public KeepaliveRequest() : base()
        {
        }
    }
}
