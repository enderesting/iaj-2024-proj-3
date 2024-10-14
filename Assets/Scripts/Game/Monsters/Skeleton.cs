using UnityEngine;
using System.Collections;
using System;
using Assets.Scripts.IAJ.Unity.Utils;
using UnityEngine.AI;
using Assets.Scripts.IAJ.Unity.DecisionMaking.BehaviorTree;
using Assets.Scripts.IAJ.Unity.DecisionMaking.BehaviorTree.BehaviourTrees;
using Assets.Scripts.IAJ.Unity.DecisionMaking.StateMachine;
using System.Collections.Generic;

namespace Assets.Scripts.Game.NPCs
{

    public class Skeleton : Monster
    {
        public Skeleton()
        {
            this.stats.Type = "Skeleton";
            this.stats.XPvalue = 3;
            this.stats.AC = 10;
            this.baseStats.HP = 5;
            this.DmgRoll = () => RandomHelper.RollD6();
            this.stats.SimpleDamage = 3;
            this.stats.AwakeDistance = 10;
            this.stats.WeaponRange = 2;

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
