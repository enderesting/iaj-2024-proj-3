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
    private AutonomousCharacter character;

    public REINFORCE(NeuralNetwork policyNetwork, float gamma, float learningRate, AutonomousCharacter character)
    {
        this.policyNetwork = policyNetwork;
        this.gamma = gamma;
        this.learningRate = learningRate;
        this.character = character;
    }

    // Train the policy using episode trajectory and the REINFORCE update rule
    public void Train(Trajectory trajectory)
    {
        // Calculate total rewards and returns (without normalization)
        float totalReward = 0f;
        for (int t = trajectory.rewards.Count - 1; t >= 0; t--)
        {
            totalReward = trajectory.rewards[t] + gamma * totalReward;
            trajectory.returns[t] = totalReward;
        }

        // Compute baseline as the mean return across the trajectory
        float baseline = trajectory.returns.Average();
        Debug.Log("baseline average:" + baseline);

        // Update policy based on the trajectory, using baseline to compute advantages
        for (int t = 0; t < trajectory.states.Count; t++)
        {
            float[] state = trajectory.states[t];
            Action action = trajectory.actions[t];
            float reward = trajectory.returns[t];

            // Calculate advantage by subtracting the baseline from the return
            float advantage = reward - baseline;

            // Calculate action probabilities for the current state
            float[] actionProbabilities = policyNetwork.Forward(state);

            // Entropy regularization (optional for better exploration)
            float entropyCoefficient = 0.02f; // Tunable parameter for entropy regularization
            float entropy = 0f;
            for (int i = 0; i < actionProbabilities.Length; i++)
            {
                if (actionProbabilities[i] > 0)
                {
                    entropy += -actionProbabilities[i] * Mathf.Log(actionProbabilities[i]);
                }
            }

            // Compute policy loss: -(log(pi) * advantage) - (entropyCoefficient * entropy)
            int actionIndex = character.Actions.IndexOf(action);
            float policyLoss = -Mathf.Log(actionProbabilities[actionIndex]) * advantage - (entropyCoefficient * entropy);
            
            Debug.Log("Advantage at step " + t + ": " + (reward - baseline));
            Debug.Log("Policy loss at step " + t + ": " + policyLoss);

            // Update the policy network weights
            policyNetwork.PolicyUpdate(action, policyLoss, state);
        }
    }
}
}

