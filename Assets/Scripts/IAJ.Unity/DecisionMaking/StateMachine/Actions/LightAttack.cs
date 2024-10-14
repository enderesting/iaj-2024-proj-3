using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.Game.NPCs;
using Assets.Scripts.Game;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.StateMachine

{
    class LightAttack : IAction
    {
        protected Monster Character { get; set; }
        public NPC Target { get; set; }


        public LightAttack(Monster character, NPC target)
        {
            this.Character = character;
            this.Target = target;
        }

        public void Execute()
        {
            if (Character.InWeaponRange(Target.gameObject))
                Character.AttackPlayer();
            else
                Debug.Log(Character.name + "out of range to attack!");
        }
    }
}
