using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.Game.NPCs;
using Assets.Scripts.Game;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.StateMachine

{
    class BreakFormation : IAction
    {
        protected Monster Character { get; set; }

        public BreakFormation(Monster character)
        {
            this.Character = character;
        }

        public void Execute()
        {
            if (Character.usingFormation) {
                Character.FormationManager.BreakFormation();
                GameManager.Instance.Formations.Clear();
            }
        }
    }
}
