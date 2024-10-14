using Assets.Scripts.Game;
using Assets.Scripts.Game.NPCs;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.StateMachine
{
    class Melee : IState
    {

        public Monster Agent { get; set; }
        public NPC Target { get; set; }

        public float maximumRange { get; set; }

        public Melee(Monster agent, NPC target)
        {
            this.Agent = agent;
            this.Target = target;
        }

        public List<IAction> GetEntryActions() { Debug.Log(Agent.name + "started melee"); return new List<IAction>(); }

        public List<IAction> GetActions()
        { return new List<IAction> { new LightAttack(Agent,Target) }; }

        public List<IAction> GetExitActions() { Debug.Log(Agent.name + "is no longer in melee"); return new List<IAction>(); }

        public List<Transition> GetTransitions()
        {
            return new List<Transition> { new LostEnemy(Agent, Target) };
        }
    }
}
