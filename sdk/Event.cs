/*

    Copyright (c) 2023 Pocketz World. All rights reserved.

*/

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Highrise.API
{
    /// <summary>
    /// Base event type
    /// </summary>
    [Serializable]
    public class Event
    {
        /// <summary>
        /// Static list of all registered event types
        /// </summary>
        protected static Dictionary<string, Type> _types = new();

        /// <summary>
        /// Type of the event
        /// </summary>
        [JsonPropertyName("_type")]
        public string Type { get; set; }

        /// <summary>
        /// Identifier of the user who sent the event
        /// </summary>
        [JsonPropertyName("user")]
        public User? Sender { get; set; }

        /// <summary>
        /// Optional tags included in the event
        /// </summary>
        [JsonPropertyName("tags")]
        public string? Tags { get; set; }

        /// <summary>
        /// Optional module the event is associated with
        /// </summary>
        [JsonPropertyName("_module")]
        public string? Module { get; set; }

        /// <summary>
        /// Parse json into an event
        /// </summary>
        public static Event? Parse(string json)
        {
            var type = JsonSerializer.Deserialize<Event>(json)?.Type;
            if (type == null)
            {
                Console.WriteLine("error: failed to parse event: " + json);
                return null;
            }

            if (!_types.TryGetValue(type, out var eventType))
            {
                Console.WriteLine("error: unknown event type: " + eventType);
                return null;
            }

            return JsonSerializer.Deserialize(json, eventType) as Event;
        }

        /// <summary>
        /// Constrct an empty event
        /// </summary>
        public Event()
        {
            Type = "";
        }

        /// <summary>
        /// Construct an empty event
        /// </summary>
        internal Event(string typeName)
        {
            Type = typeName;
        }
    }

    /// <summary>
    /// Templated event that allows for registration
    /// </summary>
    [Serializable]
    public class Event<TEvent> : Event where TEvent : Event<TEvent>, new()
    {
        /// <summary>
        /// Register the event of the given type
        /// </summary>
        public static void Register()
        {
            var a = new TEvent();
            _types[a.Type] = typeof(TEvent);
        }

        /// <summary>
        /// Construct an event with the given type name
        /// </summary>
        protected Event(string type) : base (type)
        {
        }

        /// <summary>
        /// Construct an event using the name of the class as the event type 
        /// </summary>
        public Event() : base(typeof(TEvent).Name)
        {
        }
    }
}
