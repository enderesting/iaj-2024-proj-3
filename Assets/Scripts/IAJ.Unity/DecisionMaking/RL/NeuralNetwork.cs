using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Action = Assets.Scripts.IAJ.Unity.DecisionMaking.HeroActions.Action;
using Random = UnityEngine.Random;

namespace IAJ.Unity.DecisionMaking.RL
{
    public class NeuralNetwork
    {
        private REINFORCE reinforce;
        private AutonomousCharacter character;
        public int[] layers; // Number of neurons in each layer
        private List<float[][]> weights; // Weights between layers
        private List<float[]> biases; // Biases for each layer
        private List<float[]> neurons; // Neuron values per layer
        private float discountFactor;
        private float learningRate;
        public bool InProgress;
        public Action chosenAction;
        public int numberOfVictories = 0;
        public int goldLastEpisode = 0;
        public float timeLastEpisode = 0;
        public Trajectory Trajectory;

        public enum ActivationFunction
        {
            Sigmoid,
            ReLU,
            Tanh,
        }

        private bool softmaxOutput;

        private ActivationFunction activationFunction;

        public NeuralNetwork(AutonomousCharacter character, int[] layers, float learningRate, float discountFactor,
            ActivationFunction activationFunction = ActivationFunction.Sigmoid, bool softmaxOutput = false)
        {
            this.character = character;
            this.layers = layers;
            this.learningRate = learningRate;
            this.discountFactor = discountFactor;
            this.activationFunction = activationFunction;
            this.softmaxOutput = softmaxOutput;
            InProgress = false;
            InitializeNetwork();
        }

        // Initialize weights, biases, and neurons
        private void InitializeNetwork()
        {
            reinforce = new REINFORCE(this, discountFactor, learningRate);
            
            weights = new List<float[][]>();
            biases = new List<float[]>();
            neurons = new List<float[]>();
            
            Trajectory = new Trajectory();

            for (int i = 0; i < layers.Length; i++)
            {
                neurons.Add(new float[layers[i]]);
                if (i > 0)
                {
                    biases.Add(new float[layers[i]]);
                    weights.Add(new float[layers[i]][]);

                    for (int j = 0; j < layers[i]; j++)
                    {
                        weights[i - 1][j] = new float[layers[i - 1]];
                        for (int k = 0; k < layers[i - 1]; k++)
                        {
                            weights[i - 1][j][k] = 0.1f;
                        }
                    }
                }
            }
        }

        // Activation functions
        private float Activate(float value)
        {
            switch (activationFunction)
            {
                case ActivationFunction.Sigmoid: return 1 / (1 + Mathf.Exp(-value));
                case ActivationFunction.ReLU: return Mathf.Max(0, value);
                case ActivationFunction.Tanh: return (float)Math.Tanh(value);
                default: return value;
            }
        }

        // Softmax function (used only in the output layer)
        private float[] Softmax(float[] values)
        {
            float maxVal = Mathf.Max(values); // For numerical stability
            float sumExp = 0f;
            float[] expValues = new float[values.Length];

            for (int i = 0; i < values.Length; i++)
            {
                expValues[i] = Mathf.Exp(values[i] - maxVal); // Subtract maxVal for numerical stability
                sumExp += expValues[i];
            }

            for (int i = 0; i < values.Length; i++)
            {
                expValues[i] /= sumExp;
            }

            return expValues;
        }

        private float ActivateDerivative(float value)
        {
            switch (activationFunction)
            {
                case ActivationFunction.Sigmoid: return value * (1 - value);
                case ActivationFunction.ReLU: return value > 0 ? 1 : 0;
                case ActivationFunction.Tanh: return 1 - value * value;
                default: return 1;
            }
        }

        // Forward pass
        public float[] Forward(float[] input)
        {
            Array.Copy(input, neurons[0], input.Length);

            for (int i = 1; i < layers.Length; i++)
            {
                for (int j = 0; j < layers[i]; j++)
                {
                    float sum = 0f;
                    for (int k = 0; k < layers[i - 1]; k++)
                    {
                        sum += weights[i - 1][j][k] * neurons[i - 1][k];
                    }

                    sum += biases[i - 1][j];
                    neurons[i][j] = Activate(sum);
                }
            }

            // Apply Softmax only in the output layer
            if (softmaxOutput)
            {
                neurons[^1] = Softmax(neurons[^1]);
            }

            return neurons[^1];
        }

        // Policy gradient update for REINFORCE
        public void PolicyUpdate(Action actionTaken, float reward, float[] state)
        {
            int actionIndex = character.Actions.IndexOf(actionTaken);
            
            // Forward pass
            float[] actionProbabilities = Forward(state);

            // Calculate gradient error based on selected action and reward
            float[] errors = new float[actionProbabilities.Length];
            for (int i = 0; i < errors.Length; i++)
            {
                // Policy gradient signal for action taken
                errors[i] = (i == actionIndex ? 1 : 0) - actionProbabilities[i];
                errors[i] *= reward; // Scale by reward as per REINFORCE
            }

            // Backward pass for each layer
            for (int layerIndex = weights.Count - 1; layerIndex >= 0; layerIndex--)
            {
                float[] layerErrors = new float[weights[layerIndex].Length];
        
                for (int j = 0; j < weights[layerIndex].Length; j++)
                {
                    float gradient = errors[j] * ActivateDerivative(neurons[layerIndex + 1][j]);
            
                    for (int k = 0; k < weights[layerIndex][j].Length; k++)
                    {
                        // Update weight by gradient
                        weights[layerIndex][j][k] += learningRate * gradient * neurons[layerIndex][k];
                    }
                    // Update bias
                    biases[layerIndex][j] += learningRate * gradient;
                }
        
                // Compute errors for the previous layer to propagate backward
                if (layerIndex > 0)
                {
                    for (int i = 0; i < neurons[layerIndex].Length; i++)
                    {
                        float errorSum = 0f;
                        for (int j = 0; j < weights[layerIndex].Length; j++)
                        {
                            errorSum += weights[layerIndex][j][i] * errors[j];
                        }
                        layerErrors[i] = errorSum;
                    }
                    errors = layerErrors;
                }
            }
        }

        public Action ChooseAction()
        {
            Debug.Log("Choosing action");
            // Choose an action
            Action[] executableActions = GetExecutableActions();
            Debug.Log("Number of available actions: " + executableActions.Length);
            
            Forward(CreateInputs());
            string s = "Action probabilities:";
            foreach (float p in neurons[^1]) s += " " + p;
            Debug.Log(s);
            
            float bestActionProb = float.MinValue;
            Action bestAction = null;
            for (int i = 0; i < executableActions.Length; i++)
            {
                Action action = executableActions[i];
                if (!executableActions.Contains(action)) continue;

                if (neurons[^1][i] > bestActionProb)
                {
                    bestAction = action;
                    bestActionProb = neurons[^1][i];
                }
            }
            
            InProgress = false;
            Debug.Log("Action chosen: " + bestAction.Name);
            
            Trajectory.actions.Add(bestAction);
            Trajectory.states.Add(neurons[0]);
            Trajectory.rewards.Add(0);
            Trajectory.returns.Add(0);
            return chosenAction = bestAction;
        }
        
        public Action[] GetExecutableActions()
        {
            return character.Actions.Where(a => a.CanExecute()).ToArray();
        }

        public float[] CreateInputs()
        {
            float[] inputs = new float[neurons[0].Length];

            // Character stats normalized
            inputs[0] = (float) character.baseStats.HP / character.baseStats.MaxHP; // HP
            inputs[1] = (float) character.baseStats.ShieldHP / character.baseStats.MaxShieldHp; // Shield
            inputs[2] = (float) character.baseStats.Mana / character.baseStats.MaxMana; // Mana
            inputs[3] = character.baseStats.XP / 100f; // XP
            inputs[4] = character.baseStats.Time / GameManager.GameConstants.TIME_LIMIT; // Time
            inputs[5] = character.baseStats.Money / 50f; // Money
            inputs[6] = character.baseStats.Level / 5f; // Level

            int i = 7;
            foreach (GameObject obj in GameManager.Instance.DisposableObjects.Values)
            {
                inputs[i] = (obj is not null && obj.activeInHierarchy) ? 1f : 0f;
                inputs[i + 1] = (obj is not null && obj.activeInHierarchy) ? Vector3.Distance(obj.transform.position, character.transform.position) / 250 : 1f;

                i += 2;
            }
            
            Debug.Log("Inputs: " + string.Join(" ", inputs));
            return inputs;
        }

        public void SetLastActionReward(float reward)
        {
            if (Trajectory.actions.Count <= 0) return;
            Trajectory.rewards[Trajectory.actions.Count - 1] = reward;
        }

        public void TrainEpisode()
        {
            reinforce.Train(Trajectory);
            Trajectory = new Trajectory();
            Debug.Log(ToString());
        }

        public new string ToString()
        {
            string s = "Weights:";
            foreach (float[][] w1 in weights)
            {
                s += "\n";
                foreach (float[] w2 in w1)
                {
                    s += "\n";
                    foreach (float w3 in w2)
                    {
                        s += w3.ToString() + " ";
                    }
                }
            }

            return s;
        }
    }
}