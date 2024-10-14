using Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel;
using Assets.Scripts.IAJ.Unity.Utils;
using System;
using UnityEngine;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.HeroActions
{
    public class SwordAttack : WalkToTargetAndExecuteAction
    {
        private float expectedHPChange;
        private float expectedXPChange;
        private int xpChange;
        private int enemyAC;
        private int enemySimpleDamage;
        //how do you like lambda's in c#?
        private Func<int> dmgRoll;

        public SwordAttack(AutonomousCharacter character, GameObject target) : base("SwordAttack",character,target)
        {
            if (target.tag.Equals("Skeleton"))
            {
                this.dmgRoll = () => RandomHelper.RollD6();
                this.enemySimpleDamage = 3;
                this.expectedHPChange = 3.5f;
                this.xpChange = 3;
                this.expectedXPChange = 2.7f;
                this.enemyAC = 10;
            }
            else if (target.tag.Equals("Orc"))
            {
                this.dmgRoll = () => RandomHelper.RollD10() + 2;
                this.enemySimpleDamage = 6;
                this.expectedHPChange = 7.5f;
                this.xpChange = 8;
                this.expectedXPChange = 6.0f;
                this.enemyAC = 14;
            }
            else if (target.tag.Equals("Dragon"))
            {
                this.dmgRoll = () => RandomHelper.RollD12() + RandomHelper.RollD12();
                this.enemySimpleDamage = 12;
                this.expectedHPChange = 13.0f;
                this.xpChange = 20;
                this.expectedXPChange = 10.0f;
                this.enemyAC = 18;
            }
        }

        public override float GetGoalChange(Goal goal)
        {
            var change = base.GetGoalChange(goal);

            if (goal.Name == AutonomousCharacter.SURVIVE_GOAL)
            {
                change += this.expectedHPChange;
            }
            else if (goal.Name == AutonomousCharacter.GAIN_LEVEL_GOAL)//ToDo You can add here something...
            {
                change += -this.expectedXPChange;
            }

            return change;
        }

        public override void Execute()
        {
            base.Execute();
            GameManager.Instance.SwordAttack(this.Target);
        }

 
        public override void ApplyActionEffects(WorldModel worldModel)
        {
            base.ApplyActionEffects(worldModel);

            int hp = (int)worldModel.GetProperty(PropertiesName.HP);
            int shieldHp = (int)worldModel.GetProperty(PropertiesName.ShieldHP);
            int xp = (int)worldModel.GetProperty(PropertiesName.XP);

            int damage = 0;
            if (GameManager.Instance.StochasticWorld)
            {
                //execute the lambda function to calculate received damage based on the creature type
                damage = this.dmgRoll.Invoke();
            }
            else
            {
                damage = this.enemySimpleDamage;
            }
            //calculate player's damage
            int remainingDamage = damage - shieldHp;
            int remainingShield = Mathf.Max(0, shieldHp - damage);
            int remainingHP;

            if(remainingDamage > 0)
            {
                remainingHP = hp - remainingDamage;
                worldModel.SetProperty(PropertiesName.HP, remainingHP);
            }

            worldModel.SetProperty(PropertiesName.ShieldHP, remainingShield);
 
            //calculate Hit
            //attack roll = D20 + attack modifier. Using 7 as attack modifier (+4 str modifier, +3 proficiency bonus)
            int attackRoll = RandomHelper.RollD20() + 7;

            if (attackRoll >= enemyAC || ! GameManager.Instance.StochasticWorld)
            {
                //there was an hit, enemy is destroyed, gain xp
                //disables the target object so that it can't be reused again
                worldModel.SetProperty(this.Target.name, false);
                worldModel.SetProperty(PropertiesName.XP, xp + this.xpChange);
            }
        }

        public override float GetHValue(WorldModel worldModel)
        {
            var hp = (int)worldModel.GetProperty(PropertiesName.HP);
            var maxHp = (int)worldModel.GetProperty(PropertiesName.HP);

            int xp = (int)worldModel.GetProperty(PropertiesName.XP);
            int level = (int)worldModel.GetProperty(PropertiesName.LEVEL);


            if (hp > this.expectedHPChange) // you should survive
            {
                return base.GetHValue(worldModel) * 0.5f + ((float) Math.Min(this.expectedHPChange/maxHp, 1)) * 0.3f + ((float) Math.Min(level * 10/this.expectedXPChange, 1)) * 0.2f; // normalize from 0 to 1
            }
            return 10.0f;
        }
     }
}
