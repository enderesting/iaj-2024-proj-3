using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;
using Action = Assets.Scripts.IAJ.Unity.DecisionMaking.HeroActions.Action;
using Random = UnityEngine.Random;
using System.IO;
using System.Text;

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
        private float learningRate;
        private float learningRateDecay;
        private float minLearningRate;
        private float discountFactor;
        private float exploreRate;
        private float exploreRateDecay;
        private float minExploreRate;
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

        public NeuralNetwork(AutonomousCharacter character, int[] layers,
            float learningRate, float learningRateDecay, float minLearningRate,
            float discountFactor,
            float exploreRate, float exploreRateDecay, float minExploreRate,
            ActivationFunction activationFunction=ActivationFunction.Sigmoid, bool softmaxOutput=false, string loadBrainPath=null)
        {
            this.character = character;
            this.layers = layers;
            this.learningRate = learningRate;
            this.learningRateDecay = learningRateDecay;
            this.minLearningRate = minLearningRate;
            this.discountFactor = discountFactor;
            this.exploreRate = exploreRate;
            this.exploreRateDecay = exploreRateDecay;
            this.minExploreRate = minExploreRate;
            this.activationFunction = activationFunction;
            this.softmaxOutput = softmaxOutput;
            InProgress = false;
            reinforce = new REINFORCE(this, discountFactor, learningRate);
            Trajectory = new Trajectory();
            if (loadBrainPath != null)
            {
                neurons = new List<float[]>();
                for (int i = 0; i < layers.Length; i++)
                {
                    neurons.Add(new float[layers[i]]);
                }
                LoadBrain(loadBrainPath);
            }
            else InitializeNetwork();
        }

        // Initialize weights, biases, and neurons
        private void InitializeNetwork()
        {
            weights = new List<float[][]>();
            biases = new List<float[]>();
            neurons = new List<float[]>();

            for (int i = 0; i < layers.Length; i++)
            {
                neurons.Add(new float[layers[i]]);
                
                if (i > 0)
                {
                    float[] biasLayer = new float[layers[i]];
                    float[][] weightLayer = new float[layers[i]][];

                    for (int j = 0; j < layers[i]; j++)
                    {
                        biasLayer[j] = Random.Range(-0.1f,0.1f); // Set a default value
                        weightLayer[j] = new float[layers[i - 1]];

                        for (int k = 0; k < layers[i - 1]; k++)
                        {
                            weightLayer[j][k] = Random.Range(-0.05f,0.05f); // Set a default value
                        }
                    }

                    biases.Add(biasLayer);
                    weights.Add(weightLayer);
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

            // forward pass generates probabilities for all actions
            // filters out 
            float[] allActionProbabilities = Forward(CreateInputs());
            List<float> executableActionProbs = new List<float>();
            List<int> executableActionIndices = new List<int>();

            for (int i = 0; i < allActionProbabilities.Length; i++) // loop through a list of all actions
            {
                if (executableActions.Contains(character.Actions[i]))
                {
                    executableActionProbs.Add(allActionProbabilities[i]); // add executable action index + probs
                    executableActionIndices.Add(i);
                }
            }

            string s = "Executable Action probabilities:";
            foreach (float p in neurons[^1]) s += " " + p;
            Debug.Log(s);
            
            if (Random.Range(0.0f, 1.0f) < exploreRate){
                chosenAction = executableActions[Random.Range(0, executableActions.Length)];
            }else{
                float bestActionProb = float.MinValue;
                Action bestAction = null;

                // when picking best action, just pick from executable list
                for (int i = 0; i < executableActionProbs.Count; i++)
                {
                    if (executableActionProbs[i] > bestActionProb)
                    {
                        bestAction = character.Actions[executableActionIndices[i]];
                        bestActionProb = executableActionProbs[i];
                    }
                }
                chosenAction = bestAction;
            }

                        
            InProgress = false;
            Debug.Log("Action chosen: " + chosenAction.Name);
            
            Trajectory.actions.Add(chosenAction);
            Trajectory.states.Add(neurons[0]);
            Trajectory.rewards.Add(0);
            Trajectory.returns.Add(0);
            return chosenAction;

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
            inputs[5] = character.baseStats.Money / 25f; // Money
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

        public void SaveBrain(string filePath)
        {
            NeuralNetworkData data = new NeuralNetworkData
            {
                Weights = ConvertWeights(),
                Biases = ConvertBiases()
            };

            Debug.Log("Saving Weights and Biases:");
            foreach (var layer in data.Weights)
            {
                foreach (var neuronWeights in layer)
                {
                    Debug.Log("Weight layer: " + string.Join(", ", neuronWeights));
                }
            }

            foreach (var biasLayer in data.Biases)
            {
                Debug.Log("Bias layer: " + string.Join(", ", biasLayer));
            }

            // Serialize with Newtonsoft.Json
            string json = JsonConvert.SerializeObject(data, Formatting.Indented);
            Debug.Log("JSON: " + json);
            File.WriteAllText(filePath, json, Encoding.UTF8);
            Debug.Log("Neural network saved to " + filePath);
        }

        private List<float[][]> ConvertBackWeights(List<List<List<float>>> weightsData)
        {
            List<float[][]> convertedWeights = new List<float[][]>();
            foreach (var layer in weightsData)
            {
                float[][] layerWeights = new float[layer.Count][];
                for (int i = 0; i < layer.Count; i++)
                {
                    layerWeights[i] = layer[i].ToArray();
                }
                convertedWeights.Add(layerWeights);
            }
            return convertedWeights;
        }

        private List<float[]> ConvertBackBiases(List<List<float>> biasesData)
        {
            return biasesData.Select(b => b.ToArray()).ToList();
        }

        public void LoadBrain(string filePath)
        {
            if (!File.Exists(filePath))
            {
                Debug.LogWarning("File not found: " + filePath);
                return;
            }

            string json = File.ReadAllText(filePath);
            NeuralNetworkData data = JsonConvert.DeserializeObject<NeuralNetworkData>(json);

            // Apply the loaded data to your neural network
            weights = ConvertBackWeights(data.Weights);
            biases = ConvertBackBiases(data.Biases);
            Debug.Log("Neural network loaded from " + filePath);
        }

        private List<List<List<float>>> ConvertWeights()
        {
            List<List<List<float>>> convertedWeights = new List<List<List<float>>>();
            foreach (var layer in weights)
            {
                List<List<float>> layerWeights = new List<List<float>>();
                foreach (var neuronWeights in layer)
                {
                    layerWeights.Add(neuronWeights.ToList());
                }
                convertedWeights.Add(layerWeights);
            }
            return convertedWeights;
        }

        private List<List<float>> ConvertBiases()
        {
            return biases.Select(b => b.ToList()).ToList();
        }

        public void UpdateParameters()
        {
            learningRate = Mathf.Max(minLearningRate, learningRate * Mathf.Pow(learningRateDecay, character.episodeCounter));
            exploreRate = Mathf.Max(minExploreRate, exploreRate * Mathf.Pow(exploreRateDecay, character.episodeCounter));
        }
    }
}