using System;
using System.Collections.Generic;
using UnityEngine;

namespace IAJ.Unity.DecisionMaking.RL
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    public class NeuralNetwork
    {
        public int[] layers; // Number of neurons in each layer
        private List<float[][]> weights; // Weights between layers
        private List<float[]> biases; // Biases for each layer
        private List<float[]> neurons; // Neuron values per layer
        private float learningRate;

        public enum ActivationFunction
        {
            Sigmoid,
            ReLU,
            Tanh,
            Softmax
        }

        private ActivationFunction activationFunction;

        public NeuralNetwork(int[] layers, float learningRate,
            ActivationFunction activationFunction = ActivationFunction.Sigmoid)
        {
            this.layers = layers;
            this.learningRate = learningRate;
            this.activationFunction = activationFunction;
            InitializeNetwork();
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
                    biases.Add(new float[layers[i]]);
                    weights.Add(new float[layers[i]][]);

                    for (int j = 0; j < layers[i]; j++)
                    {
                        weights[i - 1][j] = new float[layers[i - 1]];
                        for (int k = 0; k < layers[i - 1]; k++)
                        {
                            weights[i - 1][j][k] = UnityEngine.Random.Range(-1f, 1f); // Random init
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
            if (activationFunction == ActivationFunction.Softmax)
            {
                return Softmax(neurons[neurons.Count - 1]);
            }

            return neurons[neurons.Count - 1];
        }

        // Policy gradient update for REINFORCE
        public void PolicyUpdate(int actionTaken, float reward, float[] state)
        {
            float[] actionProbabilities = Forward(state);

            for (int i = 0; i < layers.Length - 1; i++)
            {
                for (int j = 0; j < layers[i + 1]; j++)
                {
                    float delta = (j == actionTaken ? 1 : 0) - actionProbabilities[j];
                    for (int k = 0; k < layers[i]; k++)
                    {
                        weights[i][j][k] += learningRate * reward * delta * neurons[i][k];
                    }

                    biases[i][j] += learningRate * reward * delta;
                }
            }
        }
    }
}