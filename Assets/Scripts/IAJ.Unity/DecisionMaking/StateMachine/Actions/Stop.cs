using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Threading.Tasks;
using UnityEngine.AI;
using Assets.Scripts.Game.NPCs;
using Assets.Scripts.Game;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.StateMachine

{
    class Stop : IAction
    {
        protected NPC Character { get; set; }


        public Stop(NPC character)
        {
            this.Character = character;
        }

        public void Execute()
        {
            Character.StopPathfinding();
        }
    }
}
