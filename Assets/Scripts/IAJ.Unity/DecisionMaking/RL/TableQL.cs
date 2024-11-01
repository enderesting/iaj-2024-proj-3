using Assets.Scripts.IAJ.Unity.DecisionMaking.HeroActions;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.RL
{
    public class TableQL
    {
        public (TQLState, int, float)[,] tableQLEntries;
        private List<TQLState> states;
        private List<Action> actions;

        public TableQL(List<Action> actions, string loadPath = null)
        {
            Debug.Log("Initializing TableQL");
            // Generate all state combinations
            this.actions = actions;
            this.states = GenerateAllStates();

            tableQLEntries = new (TQLState, int, float)[states.Count, actions.Count];

            if (loadPath != null)
            {
                LoadQTable(loadPath);
                return;
            }

            // Initialize the Q-Table with each state-action pair
            for (int i = 0; i < states.Count; i++)
            {
                for (int j = 0; j < actions.Count; j++)
                {
                    tableQLEntries[i, j] = (states[i], actions[j].ID, 0); // Initialize Qvalue to 0
                }
            }
            Debug.Log("TableQL initialized with " + states.Count + " states and " + actions.Count + " actions.");
            Debug.Log("TableQL initialized with " + tableQLEntries.Length + " entries.");
            Debug.Log("First entry" + tableQLEntries[0, 0]);
        }

        private List<TQLState> GenerateAllStates()
        {
            var states = new List<TQLState>();

            foreach (TQLState.TotalHealth health in System.Enum.GetValues(typeof(TQLState.TotalHealth)))
            {
                foreach (TQLState.Mana mana in System.Enum.GetValues(typeof(TQLState.Mana)))
                {
                    foreach (TQLState.Time time in System.Enum.GetValues(typeof(TQLState.Time)))
                    {
                        foreach (TQLState.Position position in System.Enum.GetValues(typeof(TQLState.Position)))
                        {
                            var state = new TQLState(health, mana, time, position);
                            states.Add(state);
                        }
                    }
                }                
            }

            return states;
        }

        public Action GetBestAction(TQLState currentState, List<Action> availableActions)
        {
            // Find the index of the current state in the state list
            int stateIndex = states.IndexOf(currentState);
            if (stateIndex == -1)
            {
                Debug.Log("State not found in the table.");
                return null; // Return null if the state is not found
            }

            List<Action> bestActions = new(); // List to hold actions with the best value
            float bestValue = float.MinValue;

            // Iterate through only the provided available actions for this state
            foreach (var action in availableActions)
            {
                // Retrieve the value associated with the state-action pair
                int actionIndex = actions.IndexOf(action); // Get the index of the action from the original action list
                if (actionIndex == -1) continue; // If the action is not found, skip it
                
                float value = tableQLEntries[stateIndex, actionIndex].Item3;

                // Check if the current value is better than the best found value
                if (value > bestValue)
                {
                    bestValue = value;
                    bestActions.Clear(); // Clear previous best actions
                    bestActions.Add(action); // Add the new best action
                }
                else if (value == bestValue)
                {
                    bestActions.Add(action); // Add to the list of best actions if the value matches the best
                }
            }

            // If no actions found, return null
            if (bestActions.Count == 0) return null;

            // Return a random action from the best actions
            int randomIndex = Random.Range(0, bestActions.Count);
            return bestActions[randomIndex];
        }

        public float GetQValue(TQLState state, Action action)
        {
            // Find the index of the current state in the state list
            int stateIndex = states.IndexOf(state);
            if (stateIndex == -1)
            {
                Debug.Log("State not found in the table.");
                return 0; // Return 0 if the state is not found
            }

            // Find the index of the action in the action list
            int actionIndex = actions.IndexOf(action);
            if (actionIndex == -1)
            {
                Debug.Log("Action not found in the table.");
                return 0; // Return 0 if the action is not found
            }

            // Retrieve the value associated with the state-action pair
            return tableQLEntries[stateIndex, actionIndex].Item3;
        }

        public void SetQValue(TQLState state, Action action, float value)
        {
            // Find the index of the current state in the state list
            int stateIndex = states.IndexOf(state);
            if (stateIndex == -1)
            {
                Debug.Log("State not found in the table.");
                return; // Return if the state is not found
            }

            // Find the index of the action in the action list
            int actionIndex = actions.IndexOf(action);
            if (actionIndex == -1)
            {
                Debug.Log("Action not found in the table.");
                return; // Return if the action is not found
            }

            // Retrieve the value associated with the state-action pair
            tableQLEntries[stateIndex, actionIndex].Item3 = value;
        }

        public void SaveQTable(string filePath)
        {
            List<QEntry> qEntries = new();

            // Convert the tableQLEntries into a list of QEntry objects
            for (int i = 0; i < states.Count; i++)
            {
                for (int j = 0; j < actions.Count; j++)
                {
                    var qEntry = new QEntry(
                        null, // state. we dont need this
                        tableQLEntries[i, j].Item2, // action id
                        tableQLEntries[i, j].Item3 // qvalue
                    );
                    qEntries.Add(qEntry);
                }
            }

            // Convert the list to JSON format
            string json = JsonUtility.ToJson(new SerializationWrapper<QEntry>(qEntries), true);

            // Save the JSON to a file
            File.WriteAllText(filePath, json);
        }

        public void LoadQTable(string filePath)
        {
            if (File.Exists(filePath))
            {
                Debug.Log("Loading Q-table from file: " + filePath);
                // Read the JSON data from the file
                string json = File.ReadAllText(filePath);

                // Convert the JSON back into a list of QEntry objects
                var qEntries = JsonUtility.FromJson<SerializationWrapper<QEntry>>(json).data;

                int counter = 0;
                int stateIndex = -1;
                int actionIndex = 0;
                // Disgusting looking qTable loading but it works!
                foreach (var qEntry in qEntries) // qentry cant get state so state index is always -1?
                {
                    if (counter % actions.Count == 0){
                        stateIndex += 1;
                        actionIndex = 0;
                    }

                    // stateIndex = counter % states.Count; //states.IndexOf(qEntry.state);
                    // int actionIndex = actions.FindIndex(a => a.ID == qEntry.actionID);
                    tableQLEntries[stateIndex, actionIndex].Item2 = actionIndex;
                    tableQLEntries[stateIndex, actionIndex].Item3 = qEntry.qValue;
                    counter++;
                    actionIndex++;
                }
            }
            else
            {
                Debug.LogError("File not found: " + filePath);
            }
        }
    }
}
