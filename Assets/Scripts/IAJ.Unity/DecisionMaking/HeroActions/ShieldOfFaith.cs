using Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel;
using UnityEngine;
using System;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.HeroActions
{
	public class ShieldOfFaith : Action
	{
		public int manaCost = 5;
		public int hpGain = 5;
		protected AutonomousCharacter Character { get; set; }

		public ShieldOfFaith(AutonomousCharacter character) : base("ShieldOfFaith")
		{
			this.Character = character;
		}
		
		public override float GetGoalChange(Goal goal)
		{
			float change = base.GetGoalChange(goal);

			if (goal.Name == AutonomousCharacter.SURVIVE_GOAL)
			{
				change -= Character.baseStats.MaxShieldHp - Character.baseStats.ShieldHP;
			}
			return change;
		}
		
		public override bool CanExecute()
		{
			bool res = Character.baseStats.Mana >= manaCost;
			// Debug.Log("ShieldOfFaith:" +  Character.baseStats.Mana + " mana cost:" + manaCost + " res:" + res);
			return res;
		}

		public override bool CanExecute(WorldModel worldModel){
			return (int)worldModel.GetProperty(PropertiesName.MANA) >= manaCost;
		}

		public override void Execute()
		{
			GameManager.Instance.ShieldOfFaith();
		}

        public override void ApplyActionEffects(WorldModel worldModel)
        {
            base.ApplyActionEffects(worldModel);

			int mana = (int)worldModel.GetProperty(PropertiesName.MANA);

			worldModel.SetProperty(PropertiesName.MANA, mana - this.manaCost);
			worldModel.SetProperty(PropertiesName.ShieldHP, this.hpGain);
        }

		public override float GetHValue(WorldModel worldModel)
        {
            var hp = (int)worldModel.GetProperty(PropertiesName.HP);
            var maxHp = (int)worldModel.GetProperty(PropertiesName.HP);
            // var mana = (int)worldModel.GetProperty(PropertiesName.MANA);
            var shield = (int)worldModel.GetProperty(PropertiesName.ShieldHP);
            var maxShield = (int)worldModel.GetProperty(PropertiesName.MaxShieldHP);

			float res = (float)(shield/maxShield * 0.4
				+ Math.Min(hp/maxHp,1) * 0.3
				+ base.GetHValue(worldModel) * 0.3f);
            // Debug.Log(base.ActionName + " " + res);
			// more likely to cast this if you have less hp and not enough mana for lay on hands
			// more likely if your shield is less

            return res;
        }
    }
}