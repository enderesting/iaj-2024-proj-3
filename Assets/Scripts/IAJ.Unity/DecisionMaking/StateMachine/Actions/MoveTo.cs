using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.Game.NPCs;
using Assets.Scripts.Game;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.StateMachine

{
    class MoveTo : IAction
    {
        protected NPC Character { get; set; }
        public Vector3 Target { get; set; }


        public MoveTo(NPC character, Vector3 target)
        {
            this.Character = character;
            this.Target = target;
        }

        public void Execute()
        {
            //Debug.Log(Character.name + "started pathfinding");
            Character.StartPathfinding(Target);
        }
    }
}
