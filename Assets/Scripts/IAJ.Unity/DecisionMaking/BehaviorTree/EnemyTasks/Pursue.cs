using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Threading.Tasks;
using UnityEngine.AI;
using Assets.Scripts.Game.NPCs;
using Assets.Scripts.Game;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.BehaviorTree.EnemyTasks
{
    class Pursue : Task
    {
        protected Monster Character { get; set; }

        public GameObject Target { get; set; }

        public float range;

        public Pursue(Monster character, GameObject target, float _range)
        {
            this.Character = character;
            this.Target = target;
            range = _range;
        }

        public override Result Run()
        {
            if (Target == null)
                return Result.Failure;
                
            var distance = Vector3.Distance(Character.transform.position, this.Target.transform.position);

            if (distance <= range)
            {
                return Result.Success;
            }
            else if (distance > 3f * Character.stats.AwakeDistance)
            {
                return Result.Failure;
            }
            else
            {
                Character.StartPathfinding(Target.transform.position);
                return Result.Running;
            }

        }

    }
}
