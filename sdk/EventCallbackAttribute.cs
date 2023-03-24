/*

    Copyright (c) 2023 Pocketz World. All rights reserved.

*/

namespace Highrise.API
{
    /// <summary>
    /// Marks a method a module method as being automatically registered as 
    /// an event callback
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class EventCallbackAttribute : Attribute
    {
        /// <summary>
        /// Tags required in an event for this callback to be invoked.  All 
        /// tags must be present and case is ignored.
        /// </summary>
        public List<string> RequiredTags { get; }

        /// <summary>
        /// Construct the event callback attribute with optional required tags
        /// </summary>
        /// <param name="requiredTags"></param>
        public EventCallbackAttribute(string requiredTags="")
        {
            RequiredTags = requiredTags.Split(" ", StringSplitOptions.RemoveEmptyEntries).ToList();
        }
    }
}
