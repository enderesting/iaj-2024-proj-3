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
// using System.Numerics;

namespace Assets.Scripts.Game.NPCs
{

    public class Orc : Monster
    {

        public GameObject AlertSprite;
        public List<Vector3> PatrolRoute;

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
            this.stats.BaseState = new Sleep(this); // TODO - maybe this will null out?
        }

        public override void InitializeStateMachine()
        {
            GetPatrolPositions(out Vector3 pos1,out Vector3 pos2);
            PatrolRoute.Add(pos1);
            PatrolRoute.Add(pos2);

            if (usingFormation && !formationLeader)
            {
                this.stats.BaseState = new Formation(this);
            }
            else
            {
                this.stats.BaseState = new Patrol(this,PatrolRoute);
            }

            this.StateMachine = new StateMachine(this.stats.BaseState);
    
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
                float pos2 = float.MaxValue;
                GameObject closest1 = null;
                GameObject closest2 = null;
                float temp;
                foreach (GameObject p in patrols)
                {
                    temp = Vector3.Distance(this.agent.transform.position, p.transform.position);
                    
                    if (temp < pos)
                    {
                        pos2 = pos;
                        pos = temp;
                        closest2 = closest1;
                        closest1 = p;
                    }
                    else if(temp < pos2){
                        pos2 = temp;
                        closest2 = p;
                    }
                }

            position1 = closest1.transform.position;
            position2 = closest2.transform.position;
        }

    }
}
