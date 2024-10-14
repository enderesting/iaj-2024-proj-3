using UnityEngine;
using System.Collections;
using System;
using Assets.Scripts.IAJ.Unity.Utils;
using UnityEngine.AI;
using Assets.Scripts.IAJ.Unity.DecisionMaking.BehaviorTree;
using Assets.Scripts.IAJ.Unity.DecisionMaking.BehaviorTree.BehaviourTrees;
using Assets.Scripts.IAJ.Unity.Formations;
using System.Collections.Generic;
using Assets.Scripts.IAJ.Unity.DecisionMaking.StateMachine;
using UnityEditor.PackageManager;
using System.Drawing;

namespace Assets.Scripts.Game.NPCs
{

    public class Monster : NPC

    {
        [Serializable]
        public struct EnemyStats
        {
            public string Type;
            public int XPvalue;
            public int AC;
            public int SimpleDamage;
            public float AwakeDistance;
            public float maxPursuitRange;
            public float WeaponRange;
            public IState BaseState;
        }

        public EnemyStats stats;

        public Func<int> DmgRoll;  //how do you like lambda's in c#?

        protected bool usingBehaviourTree;
        protected bool usingStateMachine;
        protected float decisionRate = 2.0f;
        protected NavMeshAgent agent;
        public GameObject Target { get; set; }
        public Task BehaviourTree;
        public StateMachine StateMachine;

        public FormationManager FormationManager;
        public bool usingFormation;
        public bool formationLeader;

        public Vector3 DefaultPosition { get; private set; }


        void Start()
        {
            agent = this.GetComponent<NavMeshAgent>();
            this.Target = GameObject.FindGameObjectWithTag("Player");

            this.usingBehaviourTree = GameManager.Instance.monsterControl == GameManager.MonsterControl.BehaviourTreeMonsters;
            this.usingStateMachine = GameManager.Instance.monsterControl == GameManager.MonsterControl.StateMachineMonsters;

            this.DefaultPosition = this.transform.position;

            if (usingBehaviourTree)
                InitializeBehaviourTree();

            if (usingStateMachine)
                InitializeStateMachine();

        }


        public virtual void InitializeBehaviourTree()
        {
            // TODO but in the children's class
        }

        public virtual void InitializeStateMachine()
        {
            // TODO but in the children's class
        }

        void FixedUpdate()
        {
            if (GameManager.Instance.gameEnded) return;
            if (usingBehaviourTree)
            {
                if (this.BehaviourTree != null)
                    this.BehaviourTree.Run();
                else
                    this.BehaviourTree = new BasicTree(this, Target);
            }
            if (usingStateMachine)
            {
                List<IAction> actions = StateMachine.Update();

                foreach (IAction action in actions) { action.Execute(); }
            }

        }

        // Monsters can attack the Main Character
        public void AttackPlayer()
        {
            GameManager.Instance.EnemyAttack(this.gameObject);
        }
        
        public bool InWeaponRange(GameObject target)
        {
            Vector3 targetDirection = target.transform.position - this.transform.position;

            RaycastHit hitInfo;
            var offset = new Vector3(0.0f, 2.0f, 0.0f); //tranform.position is at Y=0.0f
            Ray r = new(this.transform.position + offset, targetDirection * 10);

            // Sending a ray using physics and not our collider
            Physics.Raycast(r, out hitInfo);
            Debug.DrawRay(r.origin, r.direction, UnityEngine.Color.blue);

            if (hitInfo.collider != null)
            {
                // Hit something
                if (hitInfo.collider == target.GetComponent<Collider>())
                {
                    // If I hit the Target
                    Debug.Log("I am hitting the Player " + this.name);
                    return (targetDirection.magnitude <= stats.WeaponRange);
                }
                else
                {
                    // If I am hitting another thing that also has a collider
                    Debug.Log("Hitting " + hitInfo.collider.gameObject.ToString());
                    return false;
                }

            }
            else return false;
        }

        public virtual void SetFormationLeader()
        {
            // ToDo
        }
    }
}
