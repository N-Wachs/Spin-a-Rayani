namespace SpinARayan.Models
{
    public class Quest
    {
        public string Id { get; set; } = "";
        public string Description { get; set; } = "";
        public int Goal { get; set; }
        public int CurrentProgress { get; set; }
        public int RewardGems { get; set; }
        public bool IsCompleted { get; set; } = false;
        public bool IsClaimed { get; set; } = false;
        public bool IsRepeatable { get; set; } = true; // Quests können standardmäßig wiederholt werden
        public int TimesCompleted { get; set; } = 0; // Zählt wie oft die Quest abgeschlossen wurde
        public int BaseProgress { get; set; } = 0; // Basis-Fortschritt für wiederholbare Quests

        public double ProgressPercentage => (double)CurrentProgress / Goal * 100;

        public void Reset()
        {
            // Setzt die Quest zurück, erhöht aber den Basis-Fortschritt
            BaseProgress += Goal;
            CurrentProgress = 0;
            IsCompleted = false;
            IsClaimed = false;
            TimesCompleted++;
        }
    }
}
