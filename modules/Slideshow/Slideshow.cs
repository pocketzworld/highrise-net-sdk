/*

    Copyright (c) 2023 Pocketz World. All rights reserved.

*/

using System.Text.Json.Serialization;

namespace Highrise.API.Modules
{
    [Serializable]
    internal class SlideChangedEvent : Event<SlideChangedEvent>
    {
        [JsonPropertyName("slide_index")]
        public int SlideIndex { get; set; }

        [JsonPropertyName("slide_count")]
        public int SlideCount { get; set; }

        [JsonPropertyName("url")]
        public string Url { get; set; } = "";
    }

    [Serializable]
    internal class NextSlideEvent : Event<NextSlideEvent> { }

    [Serializable]
    internal class PrevSlideEvent : Event<PrevSlideEvent> { }

    /// <summary>
    /// Bot module used to present a slide show to the client.
    /// </summary>
    [RequiresModule(typeof(Storage))]
    public class Slideshow : Module
    {
        private int _current = -1;
        private Storage? _storage;
        private string[] _urls = new string[0];

        /// <inheritdoc/>
        protected override Task LoadAsync()
        {
            _storage = GetModule<Storage>();
            _urls = GetConfigStrings("slides");
            SetSlide(0);
            return Task.CompletedTask;
        }

        [EventCallback]
        private void OnPrevSlideEvent(NextSlideEvent evt) => SetSlide(_current - 1);

        [EventCallback]
        private void OnNextSlideEvent(NextSlideEvent evt) => SetSlide(_current + 1);

        private void SetSlide(int index)
        {
            index = Math.Clamp(index, 0, _urls.Length - 1);
            if (index == _current)
                return;

            _current = index;
            _storage!.SetInt(this, "count", _urls.Length);
            _storage!.SetInt(this, "current", _current);
            _storage!.SetString(this, "url", _urls[_current]);
        }
    }
}
