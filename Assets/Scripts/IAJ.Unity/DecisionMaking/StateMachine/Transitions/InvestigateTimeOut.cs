using Assets.Scripts.Game;
using Assets.Scripts.Game.NPCs;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.StateMachine
{
    class InvestigateTimeOut : Transition
    {
        private AutonomousCharacter enemy;
        public Monster agent;

        float TimeOutDuration = 20f;
        float ElapsedTime;

        public InvestigateTimeOut(Monster agent, float ElapsedTime)
        {
            this.agent = agent;
            this.enemy = GameManager.Instance.Character;
            TargetState = this.agent.stats.BaseState;
            Actions = new List<IAction>();
            this.ElapsedTime = ElapsedTime;
        }

        public override bool IsTriggered()
        {
            if (ElapsedTime >= TimeOutDuration){
                Debug.Log("timer ran out. leave");
                return true;
            }
            return false;
        }
    }
}