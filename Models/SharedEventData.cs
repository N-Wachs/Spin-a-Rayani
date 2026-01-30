using System;
using System.Text.Json.Serialization;

namespace SpinARayan.Models
{
    /// <summary>
    /// Shared event data model for multiplayer event synchronization via SupaBase.
    /// Maps to "Game Events" table in database.
    /// </summary>
    public class SharedEventData
    {
        /// <summary>
        /// Database ID (auto-generated)
        /// </summary>
        [JsonPropertyName("id")]
        public long? Id { get; set; }

        /// <summary>
        /// Event name/type
        /// </summary>
        [JsonPropertyName("event_name")]
        public string EventName { get; set; } = "Suffix Event";

        /// <summary>
        /// Suffix name for the event (e.g., "Omega", "Absolute", "Legendary")
        /// If set, this suffix will be boosted by SuffixBoostMultiplier (or default 20x)
        /// </summary>
        [JsonPropertyName("suffix_name")]
        public string? SuffixName { get; set; }
        
        /// <summary>
        /// Custom boost multiplier for the suffix (default: 20x if not set)
        /// Only used if SuffixName is set. Stored in DB as suffix_multiplier.
        /// </summary>
        [JsonPropertyName("suffix_multiplier")]
        public double? SuffixBoostMultiplier { get; set; }

        /// <summary>
        /// Username who created the event
        /// </summary>
        [JsonPropertyName("created_from")]
        public string CreatedFrom { get; set; } = "";

        /// <summary>
        /// UTC timestamp when the event starts
        /// </summary>
        [JsonPropertyName("starts_at")]
        public DateTime StartsAt { get; set; }

        /// <summary>
        /// UTC timestamp when the event ends
        /// </summary>
        [JsonPropertyName("ends_at")]
        public DateTime EndsAt { get; set; }

        /// <summary>
        /// Optional: Luck multiplier (null = no effect, 1.0 = default)
        /// </summary>
        [JsonPropertyName("luck_multiplier")]
        public float? LuckMultiplier { get; set; }

        /// <summary>
        /// Optional: Money multiplier (null = no effect, 1.0 = default)
        /// </summary>
        [JsonPropertyName("money_multiplier")]
        public float? MoneyMultiplier { get; set; }

        /// <summary>
        /// Optional: Roll time modifier (null = no effect, 1.0 = default)
        /// Lower values = faster rolling (0.5 = half cooldown, 2.0 = double cooldown)
        /// </summary>
        [JsonPropertyName("roll_time")]
        public float? RollTime { get; set; }

        /// <summary>
        /// Check if event is currently active (computed property, not stored in DB)
        /// </summary>
        [JsonIgnore]
        public bool IsActive => DateTime.UtcNow >= StartsAt && DateTime.UtcNow <= EndsAt;

        /// <summary>
        /// Get remaining time for active events (computed property, not stored in DB)
        /// </summary>
        [JsonIgnore]
        public TimeSpan RemainingTime => EndsAt - DateTime.UtcNow;
    }
}
