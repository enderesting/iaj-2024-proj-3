using System;
using System.Collections.Generic;
using UnityEngine;
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
        float totalReward = 0f;
        for (int t = trajectory.rewards.Count - 1; t >= 0; t--)
        {
            totalReward = trajectory.rewards[t] + gamma * totalReward;
            trajectory.returns[t] = totalReward;
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

