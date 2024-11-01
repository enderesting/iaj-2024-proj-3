using System.Linq;
using Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel;
using UnityEngine;
using Action = Assets.Scripts.IAJ.Unity.DecisionMaking.HeroActions.Action;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.RL
{
    public class QLearning
    {
        private float learningRate;
        private float learningRateDecay;
        private float minLearningRate;
        private float discountRate;
        private float exploreRate;
        private float exploreRateDecay;
        private float minExploreRate;
        private AutonomousCharacter character;
        private TQLState currentTQLState;
        public TableQL tableQL;
        public bool InProgress;
        public Action chosenAction;
        public int numberOfVictories = 0;
        public int goldLastEpisode = 0;
        public float timeLastEpisode = 0;

        public QLearning(float learningRate, float learningRateDecay, float minLearningRate, float discountRate, float exploreRate, float exploreRateDecay, float minExploreRate, AutonomousCharacter character, string loadpath = null)
        {
            Debug.Log("Initializing QLearning");
            this.learningRate = learningRate;
            this.learningRateDecay = learningRateDecay;
            this.minLearningRate = minLearningRate;
            this.discountRate = discountRate;
            this.exploreRate = exploreRate;
            this.exploreRateDecay = exploreRateDecay;
            this.minExploreRate = minExploreRate;
            this.character = character;
            this.tableQL = new TableQL(character.Actions, loadpath);
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


            // Get Time
            float currentTime = character.baseStats.Time;
            var time = currentTime > 100 ? TQLState.Time.High :
                           currentTime > 50 ? TQLState.Time.Medium : TQLState.Time.Low;
                           
            // Get Position
            Vector3 position = character.transform.position;
            if (position.x < 45)
            {
                if (position.z < 45)
                {
                    return new TQLState(totalHealth, mana, time, TQLState.Position.BottomLeft);
                }
                else
                {
                    return new TQLState(totalHealth, mana, time, TQLState.Position.TopLeft);
                }
            }
            else
            {
                if (position.z < 45)
                {
                    return new TQLState(totalHealth, mana, time, TQLState.Position.BottomRight);
                }
                else
                {
                    return new TQLState(totalHealth, mana, time, TQLState.Position.TopRight);
                }
            }
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

        public void UpdateParameters()
        {
            learningRate = Mathf.Max(minLearningRate, learningRate * Mathf.Pow(learningRateDecay, character.episodeCounter));
            exploreRate = Mathf.Max(minExploreRate, exploreRate * Mathf.Pow(exploreRateDecay, character.episodeCounter));
        }
    }
}