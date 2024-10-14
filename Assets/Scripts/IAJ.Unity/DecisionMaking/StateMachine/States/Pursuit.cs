using Assets.Scripts.Game.NPCs;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.StateMachine
{
    class Pursuit : IState
    {

        public Monster Agent { get; set; }
        public AutonomousCharacter Target { get; set; }

        public float maximumRange { get; set; }

        public Pursuit(Monster agent, AutonomousCharacter target)
        {
            this.Agent = agent;
            this.Target = target;
        }

        public List<IAction> GetEntryActions() {
            Debug.Log(Agent.name + "is pursuing");
            if (Agent.CompareTag("Orc"))
            {
                return new List<IAction>{
                new Shout(Agent),
                new BreakFormation(Agent)
            };
            }
            else
            {
                return new List<IAction>();
            }
        }

        public List<IAction> GetActions()
        { return new List<IAction> { new MoveTo(Agent, Target.transform.position)}; }

        public List<IAction> GetExitActions() { Debug.Log(Agent.name + "is no longer pursuing"); return new List<IAction>(); }

        public List<Transition> GetTransitions()
        {
            return new List<Transition> 
            { 
                new ToMeleeCombat(Agent,Target), 
                new LostEnemy(Agent, Target)
            };
        }
    } 
}
