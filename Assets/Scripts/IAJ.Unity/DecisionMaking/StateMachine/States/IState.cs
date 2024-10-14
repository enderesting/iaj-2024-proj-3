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
    public interface IState
    {
        public List<IAction> GetActions();

        public List<IAction> GetEntryActions();

        public List<IAction> GetExitActions();

        public List<Transition> GetTransitions();

    }
}
