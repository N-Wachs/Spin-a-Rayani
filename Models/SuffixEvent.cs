namespace SpinARayan.Models
{
    public class SuffixEvent
    {
        public long EventId { get; set; } // Database ID for tracking
        public string SuffixName { get; set; } = "";
        public string EventName { get; set; } = "";
        public string CreatedBy { get; set; } = ""; // Who created this event
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public bool IsActive => DateTime.Now >= StartTime && DateTime.Now < EndTime;
        public double BoostMultiplier { get; set; } = 20.0; // 20x häufiger (for suffix)
        
        // Event-wide multipliers (applied to all rolls during event)
        public float LuckMultiplier { get; set; } = 1.0f; // Global luck boost
        public float MoneyMultiplier { get; set; } = 1.0f; // Global money boost
        public float RollTimeModifier { get; set; } = 1.0f; // Roll speed (0.5 = 2x faster, 2.0 = 2x slower)
        
        public TimeSpan TimeRemaining => IsActive ? EndTime - DateTime.Now : TimeSpan.Zero;
        
        // Display text for ComboBox
        public string DisplayText
        {
            get
            {
                int minutes = (int)TimeRemaining.TotalMinutes;
                int seconds = TimeRemaining.Seconds;
                string creator = string.IsNullOrEmpty(CreatedBy) || CreatedBy == "Local" ? "" : $" ({CreatedBy})";
                return $"{SuffixName} - {minutes}:{seconds:D2}{creator}";
            }
        }
    }
}
