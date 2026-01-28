using System;

namespace SpinARayan.Models
{
    /// <summary>
    /// Shared event data model for multiplayer event synchronization.
    /// This is serialized to JSON and shared via cloud storage (OneDrive/Dropbox).
    /// </summary>
    public class SharedEventData
    {
        /// <summary>
        /// Suffix name for the event (e.g., "Omega", "Absolute", "Legendary")
        /// </summary>
        public string SuffixName { get; set; } = "";

        /// <summary>
        /// UTC timestamp when the event was started
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// Event duration in minutes (default: 2.5)
        /// </summary>
        public double DurationMinutes { get; set; }

        /// <summary>
        /// Username of the admin who started the event
        /// </summary>
        public string AdminName { get; set; } = "";

        /// <summary>
        /// Unique timestamp for duplicate detection
        /// </summary>
        public long Timestamp { get; set; }
    }
}
