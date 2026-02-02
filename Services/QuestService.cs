using SpinARayan.Models;

namespace SpinARayan.Services
{
    public class QuestService
    {
        public List<Quest> Quests { get; private set; } = new();
        private PlayerStats? _stats;

        public QuestService()
        {
            InitializeQuests();
        }
        
        public void LoadQuestsFromStats(PlayerStats stats)
        {
            _stats = stats;
            
            // If there are saved quests, restore them
            if (stats.SavedQuests != null && stats.SavedQuests.Count > 0)
            {
                // Merge saved quest data with initialized quests
                foreach (var savedQuest in stats.SavedQuests)
                {
                    var quest = Quests.FirstOrDefault(q => q.Id == savedQuest.Id);
                    if (quest != null)
                    {
                        quest.CurrentProgress = savedQuest.CurrentProgress;
                        quest.IsCompleted = savedQuest.IsCompleted;
                        quest.IsClaimed = savedQuest.IsClaimed;
                        quest.BaseProgress = savedQuest.BaseProgress;
                        quest.TimesCompleted = savedQuest.TimesCompleted;
                        quest.Goal = savedQuest.Goal;
                    }
                }
            }
        }
        
        public void SaveQuestsToStats()
        {
            if (_stats != null)
            {
                _stats.SavedQuests = new List<Quest>(Quests);
            }
        }

        private void InitializeQuests()
        {
            // Rolling Quests (WIEDERHOLBAR mit progressivem Goal)
            Quests.Add(new Quest
            {
                Id = "roll_100",
                Description = "Rolle 100 mal",
                Goal = 100,
                InitialGoal = 100,
                GoalIncrement = 50, // +50 bei jeder Wiederholung
                RewardGems = 100,
                IsRepeatable = true
            });
            Quests.Add(new Quest
            {
                Id = "roll_1000",
                Description = "Rolle 1000 mal",
                Goal = 1000,
                InitialGoal = 1000,
                GoalIncrement = 500, // +500 bei jeder Wiederholung
                RewardGems = 1200,
                IsRepeatable = true
            });
            Quests.Add(new Quest
            {
                Id = "roll_10000",
                Description = "Rolle 10.000 mal",
                Goal = 10000,
                InitialGoal = 10000,
                GoalIncrement = 5000, // +5000 bei jeder Wiederholung
                RewardGems = 5000,
                IsRepeatable = true
            });
            
            // Rarity Quest (EINE Quest, die mit Level hochsteigt)
            // Level wird in PlayerStats.RarityQuestLevel gespeichert
            // Dynamisch: Rarity = 1000 * (1000^level), Gems = 50 * 4^level
            Quests.Add(new Quest
            {
                Id = "rarity_milestone",
                Description = "Sammle einen Rayan mit Rarity 1.000+", // Wird dynamisch angepasst
                Goal = 1,
                InitialGoal = 1,
                GoalIncrement = 0,
                RewardGems = 50, // Wird dynamisch angepasst
                IsRepeatable = true
            });
            
            // Time-based Quests (NICHT wiederholbar)
            Quests.Add(new Quest
            {
                Id = "play_30min",
                Description = "Spiele 30 Minuten",
                Goal = 30,
                InitialGoal = 30,
                GoalIncrement = 0,
                RewardGems = 2000,
                IsRepeatable = false
            });
            Quests.Add(new Quest
            {
                Id = "play_120min",
                Description = "Spiele 2 Stunden",
                Goal = 120,
                InitialGoal = 120,
                GoalIncrement = 0,
                RewardGems = 3500,
                IsRepeatable = false
            });
            
            // Rebirth Quests (NICHT wiederholbar)
            Quests.Add(new Quest
            {
                Id = "rebirth_5",
                Description = "Rebirthe 5 mal",
                Goal = 5,
                InitialGoal = 5,
                GoalIncrement = 0,
                RewardGems = 500,
                IsRepeatable = false
            });
            Quests.Add(new Quest
            {
                Id = "rebirth_25",
                Description = "Rebirthe 25 mal",
                Goal = 25,
                InitialGoal = 25,
                GoalIncrement = 0,
                RewardGems = 2500,
                IsRepeatable = false
            });
        }

        public void UpdateProgress(PlayerStats stats)
        {
            foreach (var quest in Quests)
            {
                if (quest.IsCompleted) continue;

                int actualProgress = 0;

                if (quest.Id == "roll_100" || quest.Id == "roll_1000" || quest.Id == "roll_10000")
                {
                    actualProgress = stats.TotalRolls;
                }
                else if (quest.Id == "play_30min" || quest.Id == "play_120min")
                {
                    actualProgress = (int)stats.PlayTimeMinutes;
                }
                else if (quest.Id == "rebirth_5" || quest.Id == "rebirth_25")
                {
                    actualProgress = stats.Rebirths;
                }
                else if (quest.Id == "rarity_milestone")
                {
                    // Dynamische Rarity Quest - basiert auf RarityQuestLevel
                    int level = stats.RarityQuestLevel;
                    
                    // Calculate required rarity: 1000 * (1000^level)
                    double requiredRarity = 1000.0 * Math.Pow(1000.0, level);
                    
                    // Calculate gems reward: 50 * (4^level)
                    int gemsReward = (int)(50 * Math.Pow(4, level));
                    
                    // Update quest dynamically
                    quest.RewardGems = gemsReward;
                    quest.Description = $"Sammle einen Rayan mit Rarity {requiredRarity:N0}+ (Level {level + 1})";
                    
                    // Check if player has met the requirement
                    if (stats.BestRayanEverRarity >= requiredRarity)
                    {
                        actualProgress = 1; // Quest completed!
                    }
                }

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

                // Spezial-Behandlung für Rarity Milestone Quest
                if (quest.Id == "rarity_milestone")
                {
                    // Erhöhe das Quest-Level in den Stats
                    stats.RarityQuestLevel++;
                    Console.WriteLine($"[QuestService] Rarity Quest Level increased to {stats.RarityQuestLevel}");
                }

                // Wenn die Quest wiederholbar ist, setze sie zurück
                if (quest.IsRepeatable)
                {
                    quest.Reset();
                }
                
                // Save quest progress after claiming
                SaveQuestsToStats();
            }
        }
    }
}
