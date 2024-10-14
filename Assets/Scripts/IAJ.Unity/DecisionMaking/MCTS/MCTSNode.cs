using Assets.Scripts.IAJ.Unity.DecisionMaking.HeroActions;
using Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel;
using System.Collections.Generic;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.MCTS
{
    public class MCTSNode
    {
        public WorldModel State { get; private set; }
        public MCTSNode Parent { get; set; }
        public Action Action { get; set; }
        public int PlayerID { get; set; }
        public List<MCTSNode> ChildNodes { get; private set; }
        // number of visits
        public int N { get; set; }
        // expected Q reward
        public float Q { get; set; }
        // Q/N = "discontentment value"

        public MCTSNode(WorldModel state)
        {
            this.State = state;
            this.ChildNodes = new List<MCTSNode>();
            this.N = 0;
            this.Q = 0;
        }
    }
}
