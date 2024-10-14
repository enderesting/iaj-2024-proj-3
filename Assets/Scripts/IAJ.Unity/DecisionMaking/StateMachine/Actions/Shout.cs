using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.Game.NPCs;
using Assets.Scripts.Game;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.StateMachine

{
    class Shout : IAction
    {
        protected Monster Character { get; set; }

        public static event System.Action<Monster, Vector3> OnShout;

        public Shout(Monster character)
        {
            this.Character = character;

        }

        public void Execute()
        {
            OnShout?.Invoke(Character,Character.transform.position);
            Debug.Log(Character.name + "started shouting");
        }
    }
}
