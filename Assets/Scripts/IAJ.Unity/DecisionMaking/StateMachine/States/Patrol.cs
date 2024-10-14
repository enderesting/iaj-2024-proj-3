using Assets.Scripts.Game;
using Assets.Scripts.Game.NPCs;
using System.Collections.Generic;
using System.Resources;
using UnityEngine;
using UnityEngine.Events;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.StateMachine
{
    class Patrol : IState
    {
        public float Allowance = 0.5f;
        public Monster Agent { get; set; }

        public int CurrentTargetIndex;

        public List<Vector3> PatrolRoute;
        public Vector3 InvestigateGoal;

        public Patrol(Monster agent, List<Vector3> PatrolRoute) {
            this.Agent = agent;
            this.PatrolRoute = PatrolRoute;
            Shout.OnShout += ShoutResponse;
        }

        public List<IAction> GetEntryActions() 
        { 
            Debug.Log(Agent.name + "is starting to Patrol"); 
            return new List<IAction>(); 
        }

        public List<IAction> GetActions() {
            float dist = Vector3.Distance(Agent.transform.position,PatrolRoute[CurrentTargetIndex]);
            if (dist < Allowance){ // once reaching a point, move to the next
                CurrentTargetIndex = (CurrentTargetIndex+1)%PatrolRoute.Count;
            }
            // Debug.Log("moving to point:" + CurrentTargetIndex);
            return new List<IAction>{ new MoveTo(Agent,PatrolRoute[CurrentTargetIndex])};
        }

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
                new HeardShout(Agent,InvestigateGoal)
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
