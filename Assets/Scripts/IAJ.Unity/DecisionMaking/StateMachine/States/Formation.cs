using Assets.Scripts.Game;
using Assets.Scripts.Game.NPCs;
using System.Collections.Generic;
using System.Resources;
using UnityEngine;
using UnityEngine.Events;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.StateMachine
{
    class Formation : IState
    {
        public Monster Agent { get; set; }

        public Vector3 InvestigateGoal;

        public Formation(Monster agent) {
            this.Agent = agent;
            Shout.OnShout += ShoutResponse;
        }

        public List<IAction> GetEntryActions() 
        { 
            Debug.Log(Agent.name + "is starting a Formation"); 
            return new List<IAction>(); 
        }

        public List<IAction> GetActions() { return new List<IAction>(); }

        public List<IAction> GetExitActions() {
            this.InvestigateGoal = Vector3.zero;
            Shout.OnShout -= ShoutResponse;
            return new List<IAction>();
        }

        public List<Transition> GetTransitions()
        {
            if (Agent.CompareTag("Orc"))
            {
                return new List<Transition> {
                /*new Transition.WasAttacked,*/ 
                new EnemyDetected(Agent),
                };
            }
            return new List<Transition>();
        }

        public void ShoutResponse(Monster npc, Vector3 goalPos)
        {
            if(npc == this.Agent)
                return;
            
            Debug.Log(this.Agent.name + " has heard a shout!");
            InvestigateGoal = goalPos;
        }
    } 
}
