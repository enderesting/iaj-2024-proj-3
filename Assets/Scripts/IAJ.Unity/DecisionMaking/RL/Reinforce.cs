using System;
using System.Collections.Generic;
using UnityEngine;

namespace IAJ.Unity.DecisionMaking.RL
{
    public class REINFORCE
{
    private NeuralNetwork policyNetwork; // Neural network representing the policy
    private float gamma; // Discount factor
    private float learningRate; // Learning rate for policy update

    public REINFORCE(int[] layers, float learningRate, float gamma)
    {
        this.policyNetwork = new NeuralNetwork(layers, learningRate, NeuralNetwork.ActivationFunction.Softmax); // Using Softmax for action probabilities
        this.gamma = gamma;
        this.learningRate = learningRate;
    }

    // Sample an action based on policy network's output probabilities
    public int SampleAction(float[] state)
    {
        float[] actionProbabilities = policyNetwork.Forward(state);
        return GetSampledAction(actionProbabilities);
    }

    // Utility function to sample action based on probability distribution
    private int GetSampledAction(float[] actionProbabilities)
    {
        float rand = UnityEngine.Random.Range(0f, 1f);
        float cumulative = 0f;

        for (int i = 0; i < actionProbabilities.Length; i++)
        {
            cumulative += actionProbabilities[i];
            if (rand < cumulative)
            {
                return i;
            }
        }

        return actionProbabilities.Length - 1; // return the last action if no other was selected
    }

    // Train the policy using trajectories and the REINFORCE update rule
    public void Train(List<Trajectory> trajectories)
    {
        foreach (var trajectory in trajectories)
        {
            float totalReward = 0f;
            for (int t = trajectory.rewards.Count - 1; t >= 0; t--)
            {
                totalReward = trajectory.rewards[t] + gamma * totalReward;
                trajectory.returns[t] = totalReward;
            }
        }

        // Update policy based on the collected trajectories
        foreach (var trajectory in trajectories)
        {
            for (int t = 0; t < trajectory.states.Count; t++)
            {
                float[] state = trajectory.states[t];
                int action = trajectory.actions[t];
                float reward = trajectory.returns[t];

                // Calculate policy gradient and update the policy
                policyNetwork.PolicyUpdate(action, reward, state);
            }
        }
    }
}
}

