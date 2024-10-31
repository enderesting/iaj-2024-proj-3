using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;

namespace Assets.Scripts.Game
{
    public class NPC : MonoBehaviour
    {
        [Serializable]
        public struct Stats
        {
            public string Name;
            public int HP;
            public int ShieldHP;
            public int MaxShieldHp;
            public int MaxHP;
            public int Mana;
            public int MaxMana;
            public int XP;
            public float Time;
            public int Money;
            public int Level;

        }

        protected GameObject character;

        // Pathfinding
        public UnityEngine.AI.NavMeshAgent navMeshAgent;
        private Vector3 previousTarget;

        public Stats baseStats;
        private Stats initialStats;
        private Vector3 initialPosition;

        protected virtual void Awake()
        {
            previousTarget = new Vector3(0.0f, 0.0f, 0.0f);
            this.character = this.gameObject;
            navMeshAgent = this.GetComponent<NavMeshAgent>();
            initialStats = baseStats;
            initialPosition = this.transform.position;
        }

        public virtual void Restart()
        {
            baseStats = initialStats;
            previousTarget = new Vector3(0.0f, 0.0f, 0.0f);
            navMeshAgent.Warp(initialPosition);
        }




        #region Navmesh Pathfinding Methods

        public void StartPathfinding(Vector3 targetPosition)
        {
            //if the targetPosition received is the same as a previous target, then this a request for the same target
            //no need to redo the pathfinding search
            navMeshAgent.isStopped = false;
            if (!this.previousTarget.Equals(targetPosition))
            {

                this.previousTarget = targetPosition;

                navMeshAgent.SetDestination(targetPosition);

            }
        }

        public void StopPathfinding()
        {
            navMeshAgent.isStopped = true;
        }

        // Simple way of calculating distance left to target using Unity's navmesh
        public float GetDistanceToTarget(Vector3 originalPosition, Vector3 targetPosition)
        {
            var distance = 0.0f;

            NavMeshPath result = new();
            var r = navMeshAgent.CalculatePath(targetPosition, result);
            if (r == true)
            {
                var currentPosition = originalPosition;
                foreach (var c in result.corners)
                {
                    //Rough estimate, it does not account for shortcuts so we have to multiply it
                    distance += Vector3.Distance(currentPosition, c) * 0.95f;
                    currentPosition = c;
                }
                return distance;
            }

            //Default value
            return 100;
        }

        #endregion

    }
}
