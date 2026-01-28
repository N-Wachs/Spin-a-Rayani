using SpinARayan.Models;

namespace SpinARayan.Services
{
    public class QuestService
    {
        public List<Quest> Quests { get; private set; } = new();

        public QuestService()
        {
            InitializeQuests();
        }

        private void InitializeQuests()
        {
            // Rolling Quests
            Quests.Add(new Quest
            {
                Id = "roll_100",
                Description = "Rolle 100 mal",
                Goal = 100,
                RewardGems = 100
            });
            Quests.Add(new Quest
            {
                Id = "roll_1000",
                Description = "Rolle 1000 mal",
                Goal = 1000,
                RewardGems = 1200
            });
            Quests.Add(new Quest
            {
                Id = "roll_10000",
                Description = "Rolle 10.000 mal",
                Goal = 10000,
                RewardGems = 5000
            });
            
            // Time-based Quests
            Quests.Add(new Quest
            {
                Id = "play_30min",
                Description = "Spiele 30 Minuten",
                Goal = 30,
                RewardGems = 2000
            });
            Quests.Add(new Quest
            {
                Id = "play_120min",
                Description = "Spiele 2 Stunden",
                Goal = 120,
                RewardGems = 3500
            });
            
            // Rebirth Quests
            Quests.Add(new Quest
            {
                Id = "rebirth_5",
                Description = "Rebirthe 5 mal",
                Goal = 5,
                RewardGems = 500
            });
            Quests.Add(new Quest
            {
                Id = "rebirth_25",
                Description = "Rebirthe 25 mal",
                Goal = 25,
                RewardGems = 2500
            });
        }

        public void UpdateProgress(PlayerStats stats)
        {
            foreach (var quest in Quests)
            {
                if (quest.IsCompleted) continue;

                int actualProgress = 0;

                if (quest.Id == "roll_100" || quest.Id == "roll_1000" || quest.Id == "roll_10000")
                    actualProgress = stats.TotalRolls;
                else if (quest.Id == "play_30min" || quest.Id == "play_120min")
                    actualProgress = (int)stats.PlayTimeMinutes;
                else if (quest.Id == "rebirth_5" || quest.Id == "rebirth_25")
                    actualProgress = stats.Rebirths;

                // Berechne den Fortschritt relativ zum BaseProgress
                quest.CurrentProgress = actualProgress - quest.BaseProgress;

                if (quest.CurrentProgress >= quest.Goal && !quest.IsCompleted)
                {
                    quest.IsCompleted = true;
                }
            }
        }

        public void ClaimReward(Quest quest, PlayerStats stats)
        {
            if (quest.IsCompleted && !quest.IsClaimed)
            {
                stats.Gems += quest.RewardGems;
                quest.IsClaimed = true;

                // Wenn die Quest wiederholbar ist, setze sie zurück
                if (quest.IsRepeatable)
                {
                    quest.Reset();
                }
            }
        }
    }
}
