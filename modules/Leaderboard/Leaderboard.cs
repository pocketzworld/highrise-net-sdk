/*

    Copyright (c) 2023 Pocketz World. All rights reserved.

*/

using System.Diagnostics;
using System.Text;

namespace Highrise.API.Modules
{
    /// <summary>
    /// Bot module used to track a single leaderboard of users to score
    /// </summary>
    [RequiresModule(typeof(Storage))]
    public class Leaderboard : Module
    {
        private Stopwatch _saveTimer = new();
        private Stopwatch _syncTimer = new();
        private Dictionary<string, int> _scores = new();
        private Storage? _storage;

        /// <summary>
        /// Path of the leaderboard save file
        /// </summary>
        public string Path { get; set; } = "Leaderboard.csv";

        /// <summary>
        /// How often the leader board is saved to disk
        /// </summary>
        public int SaveInterval { get; set; } = 60;

        /// <summary>
        /// How often the leader board is saved to disk
        /// </summary>
        public int SyncInterval { get; set; } = 5;

        /// <summary>
        /// Number of scores to synchronize to the client
        /// </summary>
        public int SyncCount { get; set; } = 10;

        /// <inheritdoc/>
        protected override Task LoadAsync()
        {
            _storage = GetModule<Storage>();
            Debug.Assert(_storage != null);

            LoadScores();
            SyncScores();
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        protected override Task UnloadAsync()
        {
            SaveScores();
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        protected override Task UpdateAsync()
        {
            if (_saveTimer.IsRunning && _saveTimer.Elapsed.TotalSeconds >= SaveInterval)
                SaveScores();

            if (_syncTimer.IsRunning && _syncTimer.Elapsed.TotalSeconds >= SyncInterval)
                SyncScores();

            return Task.CompletedTask;
        }

        /// <summary>
        /// Set the score a give user
        /// </summary>
        public void SetScore(string userId, int score)
        {
            if (GetScore(userId) == score)
                return;

            _scores[userId] = score;
            if (!_saveTimer.IsRunning)
                _saveTimer.Restart();

            if (!_syncTimer.IsRunning)
                _syncTimer.Restart();
        }

        /// <summary>
        /// Add a value to the current score for a given user
        /// </summary>
        public void AddScore(string userId, int score)
        {
            SetScore(userId, GetScore(userId) + score);
        }

        /// <summary>
        /// Return the score for a give user
        /// </summary>
        public int GetScore(string userId) =>
            _scores.TryGetValue(userId, out var score) ? score : 0;

        private string BuildScoreString(IEnumerable<KeyValuePair<string,int>> scores, int count=int.MaxValue)
        {
            var builder = new StringBuilder();
            foreach (var kv in scores)
            {
                if (builder.Length > 0)
                    builder.Append(',');

                builder.Append(kv.Key);
                builder.Append(",");
                builder.Append(kv.Value);

                if (--count == 0)
                    break;
            }

            return builder.ToString();
        }

        private void SyncScores()
        {
            _syncTimer.Stop();
            _storage!.SetString(this, "scores", BuildScoreString(_scores.OrderByDescending(kv => kv.Value), SyncCount));
        }

        private void LoadScores()
        {
            _scores.Clear();

            try
            {
                var csv = File.ReadAllText(Path);
                var scores = csv.Split(',');
                for (int i = 0; i < scores.Length + 1; i += 2)
                    if (int.TryParse(scores[i + 1], out var score))
                        _scores.Add(scores[i], score);
            }            
            catch
            {

            }
        }

        private void SaveScores() 
        {
            _saveTimer.Stop();

            try
            {
                File.WriteAllText(Path, BuildScoreString(_scores));
            }
            catch
            {
            }
        }
    }
}
