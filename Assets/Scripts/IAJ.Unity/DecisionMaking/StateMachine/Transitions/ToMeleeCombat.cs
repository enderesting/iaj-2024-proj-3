using Assets.Scripts.Game;
using Assets.Scripts.Game.NPCs;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.StateMachine
{
    class ToMeleeCombat : Transition
    {
        private NPC enemy;
        public Monster agent;

        public ToMeleeCombat(Monster agent, NPC enemy)
        {
            this.agent = agent;
            this.enemy = GameManager.Instance.Character;
            TargetState = new Melee(agent, enemy);
            Actions = new List<IAction>();
        }

        public override bool IsTriggered()
        {
            return (Vector3.Distance(agent.transform.position, enemy.transform.position) <= agent.stats.WeaponRange);
        }
    }
}