using Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel;
using UnityEngine;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.HeroActions
{
	public class LayOnHands : Action
	{
		public int manaCost = 7;
		protected AutonomousCharacter Character { get; set; }

		public LayOnHands(AutonomousCharacter character) : base("LayOnHands")
		{
			this.Character = character;
		}
		
		public override float GetGoalChange(Goal goal)
		{
            var change = base.GetGoalChange(goal);

            if (goal.Name == AutonomousCharacter.SURVIVE_GOAL)
            {
                change -= goal.InsistenceValue;
            }
 
            return change;
		}
		
		public override bool CanExecute()
		{
			// Debug.Log("Lay on hands:" +  Character.baseStats.Mana + " mana cost:" + manaCost + " res:" + res);
			return Character.baseStats.Level >= 2 && 
			Character.baseStats.Mana >= manaCost && 
			Character.baseStats.HP < Character.baseStats.MaxHP;
		}

		public override bool CanExecute(WorldModel worldModel){
			return (int)worldModel.GetProperty(PropertiesName.LEVEL) >= 2 && 
			(int)worldModel.GetProperty(PropertiesName.MANA) >= manaCost && 
			(int)worldModel.GetProperty(PropertiesName.HP) < (int)worldModel.GetProperty(PropertiesName.MAXHP);
		}

		public override void Execute()
		{
			GameManager.Instance.LayOnHands();
		}

        public override void ApplyActionEffects(WorldModel worldModel)
        {
            base.ApplyActionEffects(worldModel);

			int mana = (int)worldModel.GetProperty(PropertiesName.MANA);
			int maxHp = (int)worldModel.GetProperty(PropertiesName.MAXHP);

			worldModel.SetProperty(PropertiesName.MANA, mana - this.manaCost);
			worldModel.SetProperty(PropertiesName.HP, maxHp);
        }

        public override float GetHValue(WorldModel worldModel)
        {
            var currentHP = (int)worldModel.GetProperty(PropertiesName.HP);
            var maxHP = (int)worldModel.GetProperty(PropertiesName.MAXHP);

            float res = currentHP / maxHP * 0.7f; //+ base.GetHValue(worldModel) * 0.3f;
            // ensure this always gets picked before health potion
			// Debug.Log("layonhands: " + res);
            return res;
        }
    }
}