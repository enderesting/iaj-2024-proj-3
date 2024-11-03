using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Action = Assets.Scripts.IAJ.Unity.DecisionMaking.HeroActions.Action;

namespace IAJ.Unity.DecisionMaking.RL
{
    public class REINFORCE
{
    private NeuralNetwork policyNetwork; // Neural network representing the policy
    private float gamma; // Discount factor
    private float learningRate; // Learning rate for policy update

    public REINFORCE(NeuralNetwork policyNetwork, float gamma, float learningRate)
    {
        this.policyNetwork = policyNetwork;
        this.gamma = gamma;
        this.learningRate = learningRate;
    }

    // Train the policy using episode trajectory and the REINFORCE update rule
    public void Train(Trajectory trajectory)
    {
        // Calculate total rewards and returns as usual
        float totalReward = 0f;
        for (int t = trajectory.rewards.Count - 1; t >= 0; t--)
        {
            totalReward = trajectory.rewards[t] + gamma * totalReward;
            trajectory.returns[t] = totalReward;
        }
    
        // Normalize the returns to avoid extreme gradients
        float maxReturn = trajectory.returns.Max();
        float minReturn = trajectory.returns.Min();
        for (int t = 0; t < trajectory.returns.Count; t++)
        {
            trajectory.returns[t] = (trajectory.returns[t] - minReturn) / (maxReturn - minReturn + 1e-5f); // Normalized between 0 and 1
        }


        // Update policy based on the trajectory
        for (int t = 0; t < trajectory.states.Count; t++)
        {
            float[] state = trajectory.states[t];
            Action action = trajectory.actions[t];
            float reward = trajectory.returns[t];

            // Calculate policy gradient and update the policy
            policyNetwork.PolicyUpdate(action, reward, state);
        }
    }
}
}

