/*

    Copyright (c) 2023 Pocketz World. All rights reserved.

*/

using Highrise.API.Events;
using System.Text.Json.Serialization;

namespace Highrise.API.Modules
{
    /// <summary>
    /// Defines a bot module that is used to maintain a key/value storage that
    /// is synchronized across all clients.
    /// </summary>
    public class Storage : Module
    {
        private Dictionary<string, string> _strings = new();
        private HashSet<string> _dirty = new();
        private bool _forceFullSnapshot = false;

        [Serializable]
        private struct SnapshotString
        {
            [JsonPropertyName("key")]
            public string Key { get; set; }

            [JsonPropertyName("value")]
            public string Value { get; set; }
        }

        [Serializable]
        private class SnapshotEvent : Event<SnapshotEvent>
        {
            public SnapshotString[]? strings { get; set; }
        }

        /// <inheritdoc/>
        protected override Task LoadAsync()
        {
            _forceFullSnapshot = true;
            return Task.CompletedTask;
        }

        /// <inheritdoc/>        
        protected override Task UnloadAsync()
        {
            _strings.Clear();
            return Task.CompletedTask;
        }

        /// <summary>
        /// Set the value for a given string key for the given module
        /// </summary>
        public void SetString(Module module, string key, string value) =>
            SetString(module.GetType().Name, key, value);

        /// <summary>
        /// Set the value for a given string key to an integer value for the given module
        /// </summary>
        public void SetInt (Module module, string key, int value) =>
            SetString(module.GetType().Name, key, value.ToString());

        /// <summary>
        /// Set the value for a given string key for the given module
        /// </summary>
        private void SetString(string moduleName, string key, string value)
        {
            key = moduleName + '_' + key;

            if (_strings.TryGetValue(key, out var currentValue) && value == currentValue)
                return;

            _strings[key] = value;
            _dirty.Add(key);
        }

        [EventCallback]
        private Task OnUserJoinedEvent(UserJoinedEvent evt)
        {
            _forceFullSnapshot = true;
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        protected override async Task UpdateAsync()
        {
            if (_dirty.Count == 0 && !_forceFullSnapshot)
                return;

            var strings = _strings
                .Where(kv => _forceFullSnapshot || _dirty.Contains(kv.Key))
                .Select(kv => new SnapshotString { Key = kv.Key, Value = kv.Value })
                .ToArray();

            _dirty.Clear();
            _forceFullSnapshot = false;

            await SendModuleEventAsync(new SnapshotEvent { strings = strings });
        }
    }
}
