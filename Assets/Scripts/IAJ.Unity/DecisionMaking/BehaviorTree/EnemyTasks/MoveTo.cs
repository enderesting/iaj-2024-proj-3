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
    class MoveTo : Task
    {
        protected NPC Character { get; set; }

        public Vector3 Target { get; set; }

        public float range;

        public MoveTo(Monster character, Vector3 target, float _range)
        {
            this.Character = character;
            this.Target = target;
            range = _range;
        }

        public override Result Run()
        {
            if (Target == null)
                return Result.Failure;

            if (Vector3.Distance(Character.transform.position, this.Target) <= range)
            {
                return Result.Success;
            }
            else
            {
                Character.StartPathfinding(Target);
                return Result.Running;
            }

        }

    }
}
