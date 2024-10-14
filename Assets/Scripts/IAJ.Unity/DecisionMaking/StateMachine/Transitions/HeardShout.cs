using Assets.Scripts.Game;
using Assets.Scripts.Game.NPCs;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.StateMachine
{
    class HeardShout : Transition
    {
        public Monster agent;
        public Vector3 goal;

        public HeardShout(Monster agent, Vector3 goal)
        {
            this.agent = agent;
            this.goal = goal;
            TargetState = new Investigate(agent,goal);
            Actions = new List<IAction>();
        }

        public override bool IsTriggered()
        {
            if(Vector3.Equals(goal,Vector3.zero))
                return false;
            goal = Vector3.zero;
            return true;
        }

    }
}