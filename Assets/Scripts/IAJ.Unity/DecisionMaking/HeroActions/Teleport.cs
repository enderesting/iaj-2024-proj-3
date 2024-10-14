using Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel;
using UnityEngine;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.HeroActions
{
	public class Teleport : Action
	{
		public int manaCost = 5;
		protected AutonomousCharacter Character { get; set; }

		public Teleport(AutonomousCharacter character) : base("Teleport")
		{
			this.Character = character;
		}
		
		public override float GetGoalChange(Goal goal)
		{
            var change = base.GetGoalChange(goal);

            if (goal.Name == AutonomousCharacter.BE_QUICK_GOAL)
            {
                change -= 2.0f;
            }
 
            return change;
		}
		
		public override bool CanExecute()
		{
			// Debug.Log("Teleport:" +  Character.baseStats.Mana + " mana cost:" + manaCost + " res:" + res);
			return Character.baseStats.Level >= 2 && 
			Character.baseStats.Mana >= manaCost;
		}

		public override bool CanExecute(WorldModel worldModel){
			return (int)worldModel.GetProperty(PropertiesName.LEVEL) >= 2 && 
			(int)worldModel.GetProperty(PropertiesName.MANA) >= manaCost;
		}

		public override void Execute()
		{
			GameManager.Instance.Teleport();
		}

        public override void ApplyActionEffects(WorldModel worldModel)
        {
            base.ApplyActionEffects(worldModel);

			int mana = (int)worldModel.GetProperty(PropertiesName.MANA);

			worldModel.SetProperty(PropertiesName.MANA, mana - this.manaCost);
        }
        public override float GetHValue(WorldModel worldModel)
        {
            var hp = (int)worldModel.GetProperty(PropertiesName.HP);
            var maxHp = (int)worldModel.GetProperty(PropertiesName.HP);

            int level = (int)worldModel.GetProperty(PropertiesName.LEVEL);

            float res = 0f;
			// Debug.Log("teleport: " + res);
            return res;
        }
    }
}