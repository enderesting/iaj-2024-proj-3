using Assets.Scripts.Game;
using Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel;
using UnityEngine;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.HeroActions
{
    public class PickUpChest : WalkToTargetAndExecuteAction
    {

        public PickUpChest(AutonomousCharacter character, GameObject target) : base("PickUpChest",character,target)
        {}

        public override float GetGoalChange(Goal goal)
        {
            var change = base.GetGoalChange(goal);
            if (goal.Name == AutonomousCharacter.GET_RICH_GOAL) {change -= 5.0f;}
            // Add here effects for other goals...like BeQuick...
            return change;
        }

        public override bool CanExecute()
        {

            if (!base.CanExecute())
                return false;
            return true;
        }

        public override bool CanExecute(WorldModel worldModel)
        {
            if (!base.CanExecute(worldModel)) return false;
            return true;
        }

        public override void Execute()
        {
            
            base.Execute();
            GameManager.Instance.PickUpChest(this.Target);
        }

        public override void ApplyActionEffects(WorldModel worldModel)
        {
            base.ApplyActionEffects(worldModel);

            var money = (int)worldModel.GetProperty(PropertiesName.MONEY);
            worldModel.SetProperty(PropertiesName.MONEY, money + 5);

            //disables the target object so that it can't be reused again
            worldModel.SetProperty(this.Target.name, false);
        }

        public override float GetHValue(WorldModel worldModel)
        {
            // get money property if 20 go for it
            // else return a lower value that's still decent
            // count with distance
            var money = (int)worldModel.GetProperty(PropertiesName.MONEY);
            var baseValue = base.GetHValue(worldModel);

            if (money == 20 || baseValue < 0.1) {
                return baseValue * 0.01f;
            }
            return baseValue;
        }
    }
}
