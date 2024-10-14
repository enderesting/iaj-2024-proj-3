using Assets.Scripts.IAJ.Unity.DecisionMaking.GOB;
using Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel;
using Assets.Scripts.Game;
using UnityEngine;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.HeroActions
{
    public class LevelUp : Action
    {
        public AutonomousCharacter Character { get; private set; }

        public LevelUp(AutonomousCharacter character) : base("LevelUp")
        {
            this.Character = character;
            this.Duration = AutonomousCharacter.LEVELING_INTERVAL;
        }

        public override bool CanExecute()
        {
            var level = Character.baseStats.Level;
            var xp = Character.baseStats.XP;

            return xp >= level * 10;
        }
        
        public override bool CanExecute(WorldModel worldModel)
        {
            int xp = (int)worldModel.GetProperty(PropertiesName.XP);
            int level = (int)worldModel.GetProperty(PropertiesName.LEVEL);

            return xp >= level * 10;
        }

        public override void Execute()
        {
            GameManager.Instance.LevelUp();
        }

        public override void ApplyActionEffects(WorldModel worldModel)
        {
            int maxHP = (int)worldModel.GetProperty(PropertiesName.MAXHP);
            int level = (int)worldModel.GetProperty(PropertiesName.LEVEL);
            float time = (float)worldModel.GetProperty(PropertiesName.TIME);

            worldModel.SetProperty(PropertiesName.LEVEL, level + 1);
            worldModel.SetProperty(PropertiesName.MAXHP, maxHP + 10);
            worldModel.SetProperty(PropertiesName.XP, (int)0);
            worldModel.SetProperty(PropertiesName.TIME, time + this.Duration);
        }

        public override float GetGoalChange(Goal goal)
        {
            float change = base.GetGoalChange(goal);

            if (goal.Name == AutonomousCharacter.GAIN_LEVEL_GOAL)
            {
                change = -goal.InsistenceValue;
            }
            else if (goal.Name == AutonomousCharacter.BE_QUICK_GOAL)
            {
                change += this.Duration;
            }
            return change;
        }

        public override float GetHValue(WorldModel worldModel)
        {
            // if you are close to leveling up, choose this
            int xp = (int)worldModel.GetProperty(PropertiesName.XP);
            int level = (int)worldModel.GetProperty(PropertiesName.LEVEL);
            return xp > level * 10 - 5 ? -10 : 10;
            
        }
     }
}
