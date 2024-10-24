using System.Linq;
using Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel;
using UnityEngine;
using Action = Assets.Scripts.IAJ.Unity.DecisionMaking.HeroActions.Action;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.RL
{
    public class QLearning
    {
        private float learningRate;
        private float discountRate;
        private float exploreRate;
        private AutonomousCharacter character;
        private TQLState currentTQLState;
        public TableQL tableQL;
        public bool InProgress;
        public Action chosenAction;

        public QLearning(float learningRate, float discountRate, float exploreRate, AutonomousCharacter character)
        {
            Debug.Log("Initializing QLearning");
            this.learningRate = learningRate;
            this.discountRate = discountRate;
            this.exploreRate = exploreRate;
            this.character = character;
            this.tableQL = new TableQL(character.Actions);
            InProgress = false;
        }

        public void InitializeQLearning()
        {
            InProgress = true;
        }

        public Action ChooseAction()
        {
            Debug.Log("Choosing action");
            // Choose an action
            Action[] actions = GetExecutableActions();
            Debug.Log("Number of available actions: " + actions.Length);
            Action action;

            if (Random.Range(0.0f, 1.0f) < exploreRate)
            {
                // Choose a random action
                action = actions[Random.Range(0, actions.Length)];
            }
            else
            {
                if (currentTQLState == null) currentTQLState = ConvertToTQLState();
                action = tableQL.GetBestAction(currentTQLState, actions.ToList());
            }
            InProgress = false;
            Debug.Log("Action chosen: " + action.Name);
            return chosenAction = action;
        }

        public TQLState ConvertToTQLState()
        {
            // Get Health
            int currentHealth = character.baseStats.HP;

            // Get Shield
            int currentShield = character.baseStats.ShieldHP;

            // Combine Health and Shield to get Total Health
            int currentTotalHealth = currentHealth + currentShield;
            var totalHealth = currentTotalHealth > 12 ? TQLState.TotalHealth.High :
                             currentTotalHealth > 6 ? TQLState.TotalHealth.Medium : 
                             currentTotalHealth > 3 ? TQLState.TotalHealth.Low : TQLState.TotalHealth.VeryLow;

            // Discretize Mana
            int currentMana = character.baseStats.Mana;
            var mana = currentMana > 7 ? TQLState.Mana.High :
                        currentMana > 4 ? TQLState.Mana.Medium : TQLState.Mana.Low;

            // Get Money
            int currentMoney = character.baseStats.Money;

            // Get Time
            float currentTime = character.baseStats.Time;

            // Combine Money and Time to get Progress
            float currentProgress = currentMoney / 25 - currentTime / GameManager.GameConstants.TIME_LIMIT;
            var progress = currentProgress > 0.666 ? TQLState.Progress.High :
                          currentProgress > 0.333 ? TQLState.Progress.Medium : TQLState.Progress.Low;

            // We don't care about levels higher than 2
            int level = character.baseStats.Level >= 2 ? 2 : 1;

            return new TQLState(totalHealth, mana, progress, level);
        }

        public void UpdateQValue(float reward)
        {
            Debug.Log("Updating Q-value");
            // Get the next state
            var nextTQLState = ConvertToTQLState();

            // Get the Q-value for the current state-action pair
            var qValue = tableQL.GetQValue(currentTQLState, chosenAction);

            // Get the best action for the next state
            var nextAction = tableQL.GetBestAction(nextTQLState, GetExecutableActions().ToList());

            // Get the Q-value for the next state-action pair
            var maxQValue = tableQL.GetQValue(nextTQLState, nextAction);

            // Update the Q-value for the current state-action pair
            var newQValue = (1 - learningRate) * qValue + learningRate * (reward + discountRate * maxQValue);
            tableQL.SetQValue(currentTQLState, chosenAction, newQValue);
            Debug.Log("Q-value updated: " + newQValue);
            character.AddToDiary("Q-value updated: " + newQValue);

            // Update the current state
            this.currentTQLState = nextTQLState;
        }

        public Action[] GetExecutableActions()
        {
            return character.Actions.Where(a => a.CanExecute()).ToArray();
        }
    }
}