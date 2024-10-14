using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Assets.Scripts.IAJ.Unity.Utils;
using Assets.Scripts.IAJ.Unity.DecisionMaking.HeroActions;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel
{
    //This Abstract Class defines the mehods any WorldModel should support
    public abstract class WorldModel
    {
        protected List<Action> Actions { get; set; }
        protected IEnumerator<Action> ActionEnumerator { get; set; }
        protected GameManager GameManager { get; set; }
        public AutonomousCharacter Character { get; protected set; }
        public Dictionary<string, float> GoalValues { get; set; }
        protected int NextPlayer { get; set; }
        protected Action NextEnemyAction { get; set; }
        protected Action[] NextEnemyActions { get; set; }

        public abstract WorldModel GenerateChildWorldModel();

        public abstract float GetGoalValue(string goalName);

        public abstract void SetGoalValue(string goalName, float goalValue);

        public abstract object GetProperty(string propertyName);

        public abstract void  SetProperty(string propertyName, object value);

        public abstract float GetScore();

        public abstract bool IsTerminal();

        public virtual void Initialize()
        {
            this.ActionEnumerator.Reset();
        }

        public virtual int GetNextPlayer()
        {
            return this.NextPlayer;
        }

        public virtual void CalculateNextPlayer()
        {
            Vector3 position = (Vector3)this.GetProperty(PropertiesName.POSITION);
            bool enemyEnabled;

            //basically if the character is close enough to an enemy, the next player will be the enemy.
            if (GameManager.monsterControl != GameManager.MonsterControl.SleepingMonsters)
            {
                foreach (var enemy in this.GameManager.enemies)
                {
                    enemyEnabled = (bool)this.GetProperty(enemy.name);
                    if (enemyEnabled && (enemy.transform.position - position).sqrMagnitude <= 100)
                    {
                        this.NextPlayer = 1;
                        this.NextEnemyAction = new EnemyAttack(this.GameManager.Character, enemy);
                        this.NextEnemyActions = new Action[] { this.NextEnemyAction };
                        return;
                    }
                }
            }
            this.NextPlayer = 0;
            //if not, then the next player will be player 0

        }

        public virtual Action GetNextAction()
        {
            Action action = null;
            //returns the next action that can be executed or null if no more executable actions exist
            if (this.NextPlayer == 0)
            {
                if (this.ActionEnumerator.MoveNext())
                    action = ActionEnumerator.Current;

                while (action != null && !action.CanExecute(this))
                {
                    if (this.ActionEnumerator.MoveNext())
                        action = ActionEnumerator.Current;
                    else
                        action = null;
                }
            }
            else
            {
                action = this.NextEnemyAction;
                this.NextEnemyAction = null;
            }

            return action;
        }

        public virtual Action[] GetExecutableActions()
        {
            if (this.NextPlayer == 0)
                return this.Actions.Where(a => a.CanExecute(this)).ToArray();
            else
                return this.NextEnemyActions;
        }

    }
}
