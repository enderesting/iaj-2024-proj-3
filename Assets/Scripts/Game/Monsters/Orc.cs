using UnityEngine;
using System.Collections;
using System;
using Assets.Scripts.IAJ.Unity.Utils;
using UnityEngine.AI;
using Assets.Scripts.IAJ.Unity.DecisionMaking.BehaviorTree;
using Assets.Scripts.IAJ.Unity.DecisionMaking.BehaviorTree.BehaviourTrees;
//using Assets.Scripts.IAJ.Unity.Formations;
using System.Collections.Generic;
using static GameManager;
using Assets.Scripts.IAJ.Unity.DecisionMaking.StateMachine;

namespace Assets.Scripts.Game.NPCs
{

    public class Orc : Monster
    {

        public GameObject AlertSprite;

        public Orc()
        {
            this.stats.Type = "Orc";
            this.stats.XPvalue = 8;
            this.stats.AC = 14;
            this.baseStats.HP = 15;
            this.DmgRoll = () => RandomHelper.RollD10() + 2;
            this.stats.SimpleDamage = 6;
            this.stats.AwakeDistance = 15;
            this.stats.WeaponRange = 3;
        }

        public override void InitializeStateMachine()
        {
            
            //GetPatrolPositions(out Vector3 pos1,out Vector3 pos2);

            //ToDo Create a State Machine that combines Patrol with other behaviors
            this.StateMachine = new StateMachine(new Sleep(this));
        }

        public override void InitializeBehaviourTree()
        {
            var gameObjs = GameObject.FindGameObjectsWithTag("Orc");

            this.BehaviourTree = new BasicTree(this, Target);
        }

        private void GetPatrolPositions(out Vector3 position1, out Vector3 position2)
        {
            var patrols = GameObject.FindGameObjectsWithTag("Patrol");

            float pos = float.MaxValue;
            GameObject closest = null;
            foreach (var p in patrols)
            {
                if (Vector3.Distance(this.agent.transform.position, p.transform.position) < pos)
                {
                    pos = Vector3.Distance(this.agent.transform.position, p.transform.position);
                    closest = p;
                }

            }

            position1 = closest.transform.GetChild(0).position;
            position2 = closest.transform.GetChild(1).position;
        }

    }
}
