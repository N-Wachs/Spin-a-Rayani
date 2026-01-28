namespace SpinARayan.Models
{
    public class SuffixEvent
    {
        public string SuffixName { get; set; } = "";
        public string EventName { get; set; } = "";
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public bool IsActive => DateTime.Now >= StartTime && DateTime.Now < EndTime;
        public double BoostMultiplier { get; set; } = 5.0; // 5x häufiger
        
        public TimeSpan TimeRemaining => IsActive ? EndTime - DateTime.Now : TimeSpan.Zero;
    }
}
