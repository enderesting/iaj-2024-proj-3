using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Assets.Scripts.IAJ.Unity.Utils;
using Assets.Scripts.IAJ.Unity.DecisionMaking.HeroActions;
using UnityEditor;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel
{
    //Implementation of a WorldModel Class using a recursive dictionary
    public class DictionaryWorldModel : WorldModel
    {
        private Dictionary<string, object> Properties { get; set; }
        //private bool CurrentWorld { get; set; }
        //private List<Action> Actions { get; set; }
        //protected IEnumerator<Action> ActionEnumerator { get; set; } 
        protected DictionaryWorldModel Parent { get; set; }
        //protected GameManager GameManager { get; set; }
        //protected AutonomousCharacter Character { get; set; }
        //protected int NextPlayer { get; set; }
        //protected Action NextEnemyAction { get; set; }
        //protected Action[] NextEnemyActions { get; set; }

        //This constructor is called to create the first world model,
        //that corresponds to the character's perceptions of the current "real world"
        public DictionaryWorldModel(GameManager gameManager, AutonomousCharacter character,  List<Action> actions, List<Goal> goals)
        {
            this.Properties = new Dictionary<string, object>();
            this.GoalValues = new Dictionary<string, float>();
            this.Actions = new List<Action>(actions);
            this.Actions.Shuffle();
            this.ActionEnumerator = this.Actions.GetEnumerator();
            this.GameManager = gameManager;
            this.Character = character;
            this.NextPlayer = 0;

            foreach (var goal in goals)
            {
                this.GoalValues.Add(goal.Name, goal.InsistenceValue);
            }
        }

        public DictionaryWorldModel(DictionaryWorldModel parent)
        {
            this.Properties = new Dictionary<string, object>();
            this.GoalValues = new Dictionary<string, float>();
            this.Actions = new List<Action>(parent.Actions);
            this.Actions.Shuffle();
            this.Parent = parent;
            this.ActionEnumerator = this.Actions.GetEnumerator();
            this.GameManager = parent.GameManager;
            this.Character = parent.Character;
        }

        public override WorldModel GenerateChildWorldModel()
        {
            return new DictionaryWorldModel(this);
        }

 
        public override object GetProperty(string propertyName)
        {
            //recursive implementation of WorldModel
            if (this.Properties.ContainsKey(propertyName))
            {
                return this.Properties[propertyName];
            }
            else if (this.Parent != null)
            {
                return this.Parent.GetProperty(propertyName);
            }
            else   //we are at the base WorldModel, that corresponds to the charactr's perceptions
            {
                return propertyName switch
                {
                    PropertiesName.MANA =>      this.GameManager.Character.baseStats.Mana,
                    PropertiesName.MAXMANA =>   this.GameManager.Character.baseStats.MaxMana,
                    PropertiesName.XP =>        this.GameManager.Character.baseStats.XP,
                    PropertiesName.MAXHP =>     this.GameManager.Character.baseStats.MaxHP,
                    PropertiesName.HP =>        this.GameManager.Character.baseStats.HP,
                    PropertiesName.ShieldHP =>  this.GameManager.Character.baseStats.ShieldHP,
                    PropertiesName.MaxShieldHP => this.GameManager.Character.baseStats.MaxShieldHp,
                    PropertiesName.MONEY =>     this.GameManager.Character.baseStats.Money,
                    PropertiesName.TIME =>      this.GameManager.Character.baseStats.Time,
                    PropertiesName.LEVEL =>     this.GameManager.Character.baseStats.Level,
                    PropertiesName.POSITION =>  this.GameManager.Character.gameObject.transform.position,
                    PropertiesName.DURATION =>  0.0f,
                    PropertiesName.PreviousLEVEL => 1,
                    PropertiesName.PreviousMONEY => 0,
                    _ => this.GameManager.DisposableObjects[propertyName] != null 
                    // If an object name is found in the dictionary of disposable objects, 
                    // then the object still exists. The object has been removed/destroyed otherwise.
                };
            }
        }

        public override void SetProperty(string propertyName, object value)
        {
            if (this.Parent != null) 
                this.Properties[propertyName] = value;
        }

        public override float GetGoalValue(string goalName)
        {
            //recursive implementation of WorldModel
            if (this.GoalValues.ContainsKey(goalName))
            {
                return this.GoalValues[goalName];
            }
            else if (this.Parent != null)
            {
                return this.Parent.GetGoalValue(goalName);
            }
            else  //we are at the base WorldModel, that corresponds to the charactr's perceptions
            {
                return this.GoalValues[goalName];
            }
        }

        public override void SetGoalValue(string goalName, float value)
        {
            this.GoalValues[goalName] = value;
        }

 
       /* public float CalculateDiscontentment(List<Goal> goals)
        {
            var discontentment = 0.0f;

            foreach (var goal in goals)
            {
                var newValue = this.GetGoalValue(goal.Name);

                discontentment += goal.GetDiscontentment(newValue);
            }

            return discontentment;
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
        }*/

        public override bool IsTerminal()
        {
            int HP = (int)this.GetProperty(PropertiesName.HP);
            float time = (float)this.GetProperty(PropertiesName.TIME);
            int money = (int)this.GetProperty(PropertiesName.MONEY);

            return HP <= 0 || time >= GameManager.GameConstants.TIME_LIMIT || (this.NextPlayer == 0 && money == 25);
        }

        public override float GetScore()
        {
            int money = (int)this.GetProperty(PropertiesName.MONEY);
            int HP = (int)this.GetProperty(PropertiesName.HP);
            float time = (float)this.GetProperty(PropertiesName.TIME);

            if (HP <= 0 || time >= GameManager.GameConstants.TIME_LIMIT) //lose
                return 0.0f;
            else if (this.NextPlayer == 0 && money == 25 && HP > 0) //win
                return 1.0f;
            else
            { // non-terminal state
                return 0.0f;
                //return timeAndMoneyScore(time, money) * levelScore() * hpScore(HP) * timeScore(time);
            }
        }

        private float timeAndMoneyScore(float time, int money)
        {
            float relationTimeMoney = time - 6 * money;

            if (relationTimeMoney > 30)
                return 0;
            else if (relationTimeMoney < 0)
                return 0.6f;
            else
                return 0.3f;
        }

        private float timeScore(float time)
        {
            return (1 - time / GameManager.GameConstants.TIME_LIMIT);
        }

        private float levelScore()
        {
            int level = (int)this.GetProperty(PropertiesName.LEVEL);
            if (level == 2)
                return 1f;
            else if (level == 1)
                return 0.4f;
            else
                return 0;
        }

        private float hpScore(int hp)
        {
            if (hp > 18) //survives orc and dragon
                return 1f;
            if (hp > 12) //survives dragon or two orcs
                return 0.6f;
            else if (hp > 6) //survives orc
                return 0.1f;
            else
                return 0.01f;

        }


        /*public virtual int GetNextPlayer()
        {
            return this.NextPlayer;
        }

        public virtual void CalculateNextPlayer()
        {
            Vector3 position = (Vector3)this.GetProperty(PropertiesName.POSITION);
            bool enemyEnabled;

            //basically if the character is close enough to an enemy, the next player will be the enemy.
            foreach (var enemy in this.GameManager.enemies)
            {
                enemyEnabled = (bool)this.GetProperty(enemy.name);
                if (enemyEnabled && (enemy.transform.position - position).sqrMagnitude <= 100)
                {
                    this.NextPlayer = 1;
                    this.NextEnemyAction = new SwordAttack(this.GameManager.Character, enemy);
                    this.NextEnemyActions = new Action[] { this.NextEnemyAction };
                    return;
                }
            }
            this.NextPlayer = 0;
            //if not, then the next player will be player 0

        }*/
    }
}