
using Assets.Scripts.Game;
using Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel;
using Assets.Scripts.IAJ.Unity.Utils;
using System;
using UnityEngine;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.HeroActions
{
    public class EnemyAttack : SwordAttack
    {

        public EnemyAttack(AutonomousCharacter character, GameObject target) : base(character, target)
        {
            this.Name = "EnemyAttack(" + target.name + ")";
        }
       
        public override void Execute()
        {
            base.Execute();
            GameManager.Instance.EnemyAttack(this.Target);
        }

        public override float GetHValue(WorldModel worldModel)
        {
            return -10; // make sure this action is always chosen so that the player doesn't think he can avoid the attack
        }
    }
}
