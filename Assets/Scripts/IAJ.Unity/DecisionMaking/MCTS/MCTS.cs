using Assets.Scripts.IAJ.Unity.DecisionMaking.HeroActions;
using Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Action = Assets.Scripts.IAJ.Unity.DecisionMaking.HeroActions.Action;
using Assets.Scripts.IAJ.Unity.Utils;
using UnityEditor.Animations;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.MCTS
{
    public class MCTS
    {
        public const float C = 1.4f;
        public bool InProgress { get; private set; }
        protected int MaxIterations { get; set; }
        protected int MaxIterationsPerFrame { get; set; }
        protected int NumberPlayouts { get; set; }
        protected int PlayoutDepthLimit { get; set; }
        public MCTSNode BestFirstChild { get; set; }

        public List<MCTSNode> BestSequence { get; set; }
        public WorldModel BestActionSequenceEndState { get; set; }
        public int CurrentIterations { get; protected set; }
        protected int CurrentDepth { get; set; }
        protected int FrameCurrentIterations { get; set; }
        protected WorldModel InitialState { get; set; }
        protected MCTSNode InitialNode { get; set; }
        protected System.Random RandomGenerator { get; set; }
        
        //Information and Debug Properties
        public int MaxPlayoutDepthReached { get; set; }
        public int MaxSelectionDepthReached { get; set; }
        public float TotalProcessingTime { get; set; }
        
        //public List<Action> BestActionSequence { get; set; }
        //Debug
         

        public MCTS(WorldModel currentStateWorldModel, int maxIter, int maxIterFrame, int playouts, int playoutDepthLimit)
        {
            this.InitialState = currentStateWorldModel;
            this.MaxIterations = maxIter;
            this.MaxIterationsPerFrame = maxIterFrame;
            this.NumberPlayouts = playouts;
            this.PlayoutDepthLimit = playoutDepthLimit;
            this.InProgress = false;
            this.RandomGenerator = new System.Random();
        }

        public void InitializeMCTSearch()
        {
            this.InitialState.Initialize();
            this.MaxPlayoutDepthReached = 0;
            this.MaxSelectionDepthReached = 0;
            this.CurrentIterations = 0;
            this.FrameCurrentIterations = 0;
            this.TotalProcessingTime = 0.0f;
 
            // create root node n0 for state s0
            this.InitialNode = new MCTSNode(this.InitialState)
            {
                Action = null,
                Parent = null,
                PlayerID = 0
            };
            this.InProgress = true;
            this.BestFirstChild = null;
            //this.BestActionSequence = new List<Action>();
        }

        public Action ChooseAction()
        {
            MCTSNode selectedNode;
            float reward;

            var startTime = Time.realtimeSinceStartup;

            //while within computational budget

            //Selection + Expansion

            //Playout

            //Backpropagation

            // return best initial child
            return null;
        }

        // Selection and Expantion
        protected MCTSNode Selection(MCTSNode initialNode)
        {
            Action nextAction;
            MCTSNode currentNode = initialNode;
            MCTSNode bestChild;

            //   while(!currentNode.State.IsTerminal())

            // nextAction = currentNode.State.GetNextAction();

            //Expansion

            return null;
        }

        protected virtual float Playout(WorldModel initialStateForPlayout)
        {
            Action[] executableActions;

            //ToDo
            return 0.0f;
        }

        protected virtual void Backpropagate(MCTSNode node, float reward)
        {
            //ToDo, do not forget to later consider two advesary moves...
        }

        protected MCTSNode Expand(MCTSNode parent, Action action)
        {
            WorldModel newState = parent.State.GenerateChildWorldModel();
            //ToDo
            return null;
        }

        protected virtual MCTSNode BestUCTChild(MCTSNode node)
        {
            //ToDo
            return null;
        }

        //this method is very similar to the bestUCTChild, but it is used to return the final action of the MCTS search, and so we do not care about
        //the exploration factor
        protected MCTSNode BestChild(MCTSNode node)
        {
            //ToDo
            return null;
        }


        protected Action BestAction(MCTSNode node)
        {
            var bestChild = this.BestChild(node);
            if (bestChild == null) return null;

            this.BestFirstChild = bestChild;

            //this is done for debugging proposes only
            this.BestSequence = new List<MCTSNode> { bestChild };
            node = bestChild;
            this.BestActionSequenceEndState = node.State;

            while(!node.State.IsTerminal())
            {
                bestChild = this.BestChild(node);
                if (bestChild == null) {
                    break;
                }
                this.BestSequence.Add(bestChild);
                node = bestChild;
                this.BestActionSequenceEndState = node.State;
            }
            return this.BestFirstChild.Action;
        }

    }
}
