using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Threading.Tasks;
using UnityEngine.AI;
using Assets.Scripts.Game.NPCs;
using Assets.Scripts.Game;
using System.Xml.Linq;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.StateMachine
{
    public class StateMachine
    {
        
        public IState CurrentState { get; private set; }
        
        public StateMachine(IState initialState)
        {
            CurrentState = initialState;
        }

        //Checks and applies transitions, returning a list of actions
        public List<IAction> Update()
        {
            Transition triggered = null;

            foreach (var transition in CurrentState.GetTransitions())
            {
                if ( transition.IsTriggered() )
                {
                    triggered = transition;
                    break;
                }
            }

            if ( triggered != null )
            {
                IState targetState = triggered.TargetState;
                //Add the exit action of the old state, the transition action and the entry for the new state.
                var actions = CurrentState.GetExitActions();
                actions.AddRange(triggered.Actions);
                actions.AddRange(targetState.GetEntryActions());

                //Complete the transition and return the actions to execute
                CurrentState = targetState;
                return actions;
            }
            else
            {
                return CurrentState.GetActions();
            }
        }

    }
}
