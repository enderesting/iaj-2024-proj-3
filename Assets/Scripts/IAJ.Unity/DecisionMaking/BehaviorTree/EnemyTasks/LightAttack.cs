using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Threading.Tasks;
using Assets.Scripts.Game.NPCs;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.BehaviorTree.EnemyTasks
{
    class LightAttack : Task
    {
        protected Monster character { get; set; }

        public GameObject target { get; set; }

        public LightAttack(Monster character)
        {
            this.character = character;
        }
        public override Result Run()
        {
            character.AttackPlayer();
            return Result.Success;
        }

    }
}
