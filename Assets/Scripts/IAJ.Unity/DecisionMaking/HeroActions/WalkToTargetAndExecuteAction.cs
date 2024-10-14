using UnityEngine;
using Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel;
using System;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.HeroActions
{
    public abstract class WalkToTargetAndExecuteAction : Action
    {
        protected AutonomousCharacter Character { get; set; }

        public GameObject Target { get; set; }

        protected WalkToTargetAndExecuteAction(string actionName, AutonomousCharacter character, GameObject target) : base(actionName + "(" + target.name + ")")
        {
            this.Character = character;
            this.Target = target;
        }

        public override float GetDuration()
        {
            return this.GetDuration(this.Character.transform.position);
        }

        public override float GetDuration(WorldModel worldModel)
        {
            var position = (Vector3)worldModel.GetProperty(PropertiesName.POSITION);
            return this.GetDuration(position);
        }

        private float GetDuration(Vector3 currentPosition)
        {
            var distance = getDistance(currentPosition, Target.transform.position);
            var result = distance / AutonomousCharacter.SPEED;
            return result;
        }

        public override float GetGoalChange(Goal goal)
        {
            if (goal.Name == AutonomousCharacter.BE_QUICK_GOAL)
            {
                return this.GetDuration();
            }
            else return 0.0f;
        }

        public override bool CanExecute()
        {
            return this.Target.activeInHierarchy;
        }

        public override bool CanExecute(WorldModel worldModel)
        {
            if (this.Target == null) return false;
            var targetEnabled = (bool)worldModel.GetProperty(this.Target.name);
            return targetEnabled;
        }

        public override void Execute()
        {
            Vector3 delta = this.Target.transform.position - this.Character.transform.position;
            
            if (delta.sqrMagnitude > 5 )
               this.Character.StartPathfinding(this.Target.transform.position);
        }

        public override void ApplyActionEffects(WorldModel worldModel)
        {
            var duration = this.GetDuration(worldModel);

            var time = (float)worldModel.GetProperty(PropertiesName.TIME);
            worldModel.SetProperty(PropertiesName.DURATION, duration);
            worldModel.SetProperty(PropertiesName.TIME, time + duration);
            worldModel.SetProperty(PropertiesName.POSITION, Target.transform.position);
        }

        private float getDistance(Vector3 currentPosition, Vector3 targetPosition)
        {        
            
            var distance = this.Character.GetDistanceToTarget(currentPosition, targetPosition);
            return distance;
        }

        public override float GetHValue(WorldModel worldModel)
        {
            var position = (Vector3)worldModel.GetProperty(PropertiesName.POSITION);
            var duration = (position - this.Target.transform.position).magnitude / AutonomousCharacter.SPEED; // avoid navmesh bug
            var time = (float) worldModel.GetProperty(PropertiesName.TIME);
            return (float) (Math.Log(duration + 1)/Math.Log(150 - time + 1));
        }
    }
}