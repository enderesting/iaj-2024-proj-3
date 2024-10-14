using Assets.Scripts.Game;
using Assets.Scripts.Game.NPCs;
using System.Collections.Generic;
using System.Resources;
using UnityEngine;
using UnityEngine.Events;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.StateMachine
{
    class Investigate : IState
    {
        public float Allowance = 0.5f;

        public Monster Agent { get; set; }

        public Vector3 Goal;
        
        public float ElapsedTime;

        public Investigate(Monster agent, Vector3 goal) {
            this.Agent = agent;
            this.Goal = goal;
        }

        public List<IAction> GetEntryActions() 
        { 
            Debug.Log(Agent.name + "is starting to Investigate the Alert!!!");
            this.ElapsedTime = 0; 
            return new List<IAction>(); 
        }

        public List<IAction> GetActions() {
            this.ElapsedTime += Time.deltaTime;
            return new List<IAction>{

                new MoveTo(Agent,Goal)
            };
        }

        public List<IAction> GetExitActions() {
            Debug.Log(Agent.name + "is no longer Investigating."); 
            return new List<IAction>(); }

        public List<Transition> GetTransitions()
        {
            return new List<Transition> {
                new EnemyDetected(Agent),
                new InvestigateTimeOut(Agent,ElapsedTime),
            };
        }
    } 
}
