using Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel;
using UnityEngine;
using System;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.HeroActions
{
	public class DivineSmite : WalkToTargetAndExecuteAction
	{
		private int manaCost = 2;
		private AutonomousCharacter character;
		private GameObject target;
		private int xpChange = 3;
		private float expectedXPChange = 2.7f;

		private float expectedHPChange = 3.5f; //avg skeleton damage

		public DivineSmite(AutonomousCharacter character, GameObject target) : base("DivineSmite", character, target)
		{
			this.character = character;
			this.target = target;
		}

		public override bool CanExecute()
		{
			return target != null && target.activeInHierarchy && target.CompareTag("Skeleton") && this.character.baseStats.Mana >= 2;
		}

		public override bool CanExecute(WorldModel worldModel){
			
			return target != null && target.activeInHierarchy && target.CompareTag("Skeleton")
					 && (int)worldModel.GetProperty(PropertiesName.MANA) >= manaCost;
		}

		public override void Execute()
		{
			base.Execute();
			GameManager.Instance.DivineSmite(target);
		}

		public override float GetGoalChange(Goal goal)
		{
			float change = 0.0f;
			
			if (goal.Name == AutonomousCharacter.GAIN_LEVEL_GOAL)
			{
				change += -this.expectedXPChange;
			}

			return change;
		}

        public override void ApplyActionEffects(WorldModel worldModel)
        {
            base.ApplyActionEffects(worldModel);

			int xp = (int)worldModel.GetProperty(PropertiesName.XP);
			int mana = (int)worldModel.GetProperty(PropertiesName.MANA);

			worldModel.SetProperty(this.Target.name, false);
            worldModel.SetProperty(PropertiesName.XP, xp + this.xpChange);
			worldModel.SetProperty(PropertiesName.MANA, mana - this.manaCost);

        }

        public override float GetHValue(WorldModel worldModel)
        {
            var hp = (int)worldModel.GetProperty(PropertiesName.HP);
            var maxHp = (int)worldModel.GetProperty(PropertiesName.HP);

            int level = (int)worldModel.GetProperty(PropertiesName.LEVEL);

            float res;

			// always choose if executable + your character has less hp than the enemy, skeleton expected 3.5f
			if (hp <= this.expectedHPChange) // if hp lesser than avg skeleton damage, always execute
			{
				res = 0;
			}else{
                res = base.GetHValue(worldModel) * 0.5f
                    + ((float) Math.Min(this.expectedHPChange/maxHp, 1)) * 0.3f
                    + ((float) Math.Min(level * 10/this.expectedXPChange, 1)) * 0.2f; // normalize from 0 to 1
			}

            return res;
        }

    }
}