using UnityEngine;
using System.Collections;
using System;
using Assets.Scripts.IAJ.Unity.Utils;
using UnityEngine.AI;
using Assets.Scripts.IAJ.Unity.DecisionMaking.BehaviorTree;
using Assets.Scripts.IAJ.Unity.DecisionMaking.BehaviorTree.BehaviourTrees;
using System.Collections.Generic;
using Assets.Scripts.IAJ.Unity.DecisionMaking.StateMachine;

namespace Assets.Scripts.Game.NPCs
{

    public class Dragon : Monster
    {
        public Dragon()
        {
            this.stats.Type = "Dragon";
            this.stats.XPvalue = 20;
            this.stats.AC = 16;
            this.baseStats.HP = 30;
            this.DmgRoll = () => RandomHelper.RollD12() + RandomHelper.RollD12()+3;
            this.stats.SimpleDamage = 15;
            this.stats.AwakeDistance = 20;
            this.stats.WeaponRange = 15;
            this.stats.BaseState = new Sleep(this);
        }

        public override void InitializeBehaviourTree()
        {
            this.BehaviourTree = new BasicTree(this, Target);
        }

        public override void InitializeStateMachine()
        {
            this.StateMachine = new StateMachine(new Sleep(this));
        }



    }
}
