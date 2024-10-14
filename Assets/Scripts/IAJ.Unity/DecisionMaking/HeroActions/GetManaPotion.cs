using Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel;
using UnityEngine;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.HeroActions
{
    public class GetManaPotion : WalkToTargetAndExecuteAction
    {
        public GetManaPotion(AutonomousCharacter character, GameObject target) : base("GetManaPotion",character,target)
        {
        }

        public override bool CanExecute()
        {
            if (!base.CanExecute()) return false;
            return Character.baseStats.Mana < Character.baseStats.MaxMana;
        }

        public override bool CanExecute(WorldModel worldModel)
        {
            if (!base.CanExecute(worldModel)) return false;

            var currentMana = (int)worldModel.GetProperty(PropertiesName.MANA);
            var maxMana = (int)worldModel.GetProperty(PropertiesName.MAXMANA);
            return currentMana < maxMana;
        }

        public override void Execute()
        {
            base.Execute();
            GameManager.Instance.GetManaPotion(this.Target);
        }

        public override float GetGoalChange(Goal goal)
        {
            var change = base.GetGoalChange(goal);

            if (goal.Name == AutonomousCharacter.SURVIVE_GOAL)
            {
                change -= goal.InsistenceValue;
            }
            if (goal.Name == AutonomousCharacter.GAIN_LEVEL_GOAL)
            {
                change -= goal.InsistenceValue;
            }
 
            return change;
        }

        public override void ApplyActionEffects(WorldModel worldModel)
        {
            base.ApplyActionEffects(worldModel);
            var maxMana = (int)worldModel.GetProperty(PropertiesName.MAXMANA);
            worldModel.SetProperty(PropertiesName.MANA, maxMana);

            //disables the target object so that it can't be reused again
            worldModel.SetProperty(this.Target.name, false);
        }

        public override float GetHValue(WorldModel worldModel)
        {
            var currentMana = (int)worldModel.GetProperty(PropertiesName.MANA);
            var maxMana = (int)worldModel.GetProperty(PropertiesName.MAXMANA);

            float res = currentMana / maxMana * 0.5f + base.GetHValue(worldModel) * 0.5f;

            // Debug.Log(base.ActionName + " " + res);
            return res;
        }
    }
}
