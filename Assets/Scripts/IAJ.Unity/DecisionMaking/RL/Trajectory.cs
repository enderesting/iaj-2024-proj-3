using System.Collections.Generic;
using Assets.Scripts.IAJ.Unity.DecisionMaking.HeroActions;

namespace IAJ.Unity.DecisionMaking.RL
{
    // Class representing a trajectory for storing states, actions, and rewards
    public class Trajectory
    {
        public List<float[]> states; // States visited
        public List<Action> actions; // Actions taken
        public List<float> rewards; // Rewards received
        public List<float> returns; // Discounted returns

        public Trajectory()
        {
            states = new List<float[]>();
            actions = new List<Action>();
            rewards = new List<float>();
            returns = new List<float>();
        }
    }
}