using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Threading.Tasks;
using UnityEngine.AI;
using Assets.Scripts.Game.NPCs;
using Assets.Scripts.Game;
using System.Xml.Linq;
using JetBrains.Annotations;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.StateMachine
{
    public class Transition
    {
        
        public IState TargetState;
        public List<IAction> Actions;

        public virtual bool IsTriggered()
        { return false; }

    }
}
