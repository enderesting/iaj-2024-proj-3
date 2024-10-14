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
        public float ProcessingTime { get; set; }
        
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
            this.RandomGenerator = new System.Random(1);
            this.TotalProcessingTime = 0.0f;
        }

        public void InitializeMCTSearch()
        {
            this.InitialState.Initialize();
            this.MaxPlayoutDepthReached = 0;
            this.MaxSelectionDepthReached = 0;
            this.CurrentIterations = 0;
            this.FrameCurrentIterations = 0;
            this.ProcessingTime = 0.0f;
 
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
            FrameCurrentIterations = 0;
            int playoutCount = 0;

            while (this.CurrentIterations < this.MaxIterations)
            {
                if (FrameCurrentIterations >= this.MaxIterationsPerFrame)
                {
                    this.TotalProcessingTime += Time.realtimeSinceStartup - startTime;
                    this.ProcessingTime += Time.realtimeSinceStartup - startTime;
                    return null;    
                }

                int maxSelectionDepth = 0;
                // Selection (& Expansions)
                selectedNode = Selection(this.InitialNode, ref maxSelectionDepth);
                
                if (maxSelectionDepth > this.MaxSelectionDepthReached)
                    this.MaxSelectionDepthReached = maxSelectionDepth;

                // Playout
                reward = Playout(selectedNode.State, ref playoutCount);

                // Backpropagation
                Backpropagate(selectedNode, reward);
                this.CurrentIterations++;
                this.FrameCurrentIterations++;
            }

            // return best initial child
            this.InProgress = false;
            Action bestAction = BestAction(this.InitialNode);
            this.TotalProcessingTime += Time.realtimeSinceStartup - startTime;
            this.ProcessingTime += Time.realtimeSinceStartup - startTime;
            return bestAction;
        }

        // Selection and Expansion
        // edit: based on https://youtu.be/UXW2yZndl7U about 1:10 in
        protected virtual MCTSNode Selection(MCTSNode initialNode, ref int maxSelectionDepth)
        {
            // selecting from existing tree the best node based on an equation
            // expand: whichever best node it lands on, find an unexplored action and explore the node
            Action nextAction;
            MCTSNode currentNode = initialNode;

            while (!currentNode.State.IsTerminal())
            {
                maxSelectionDepth++;
                // not fully explored
                if ((nextAction = currentNode.State.GetNextAction()) is not null)
                {
                    return Expand(currentNode, nextAction);
                }

                currentNode = BestUCTChild(currentNode);
            }

            return currentNode;
        }

        protected virtual float Playout(WorldModel initialStateForPlayout, ref int playoutCount)
        {
            WorldModel currentState = initialStateForPlayout;
            Action[] executableActions;
            float reward = 0;
            int playoutDepth = 0;
            while (!currentState.IsTerminal() && playoutDepth < this.PlayoutDepthLimit
                    && playoutCount < NumberPlayouts)
            {
                WorldModel newState = currentState.GenerateChildWorldModel();
                
                executableActions = newState.GetExecutableActions();

                Action selectedAction = executableActions[this.RandomGenerator.Next(0, executableActions.Length)];

                selectedAction.ApplyActionEffects(newState);
                currentState.CalculateNextPlayer();
                currentState = newState;
                playoutDepth++;
                playoutCount++;
                // Debug.Log("action chosen:" + selectedAction);
            }

            if (playoutDepth > this.MaxPlayoutDepthReached)
                this.MaxPlayoutDepthReached = playoutDepth;

            reward = currentState.GetScore();
            return currentState.GetScore();
        }

        protected virtual void Backpropagate(MCTSNode node, float reward)
        {
            // Debug.Log("reward:" + reward);
            node.Q += reward;
            node.N += 1;
            MCTSNode currentNode = node;
            while (currentNode.Parent != null){
                currentNode.Parent.Q += reward;
                currentNode.Parent.N += 1;
                currentNode = currentNode.Parent;
            }
        }

        // given a parent node and action, apply the action and adds a new node to the tree
        protected MCTSNode Expand(MCTSNode parent, Action action)
        {
            WorldModel newState = parent.State.GenerateChildWorldModel();
            
            action.ApplyActionEffects(newState); //apply action to wm like this

            MCTSNode newNode = new MCTSNode(newState);
            newNode.Action = action;
            newNode.Parent = parent;
            parent.ChildNodes.Add(newNode);
            return newNode;
        }

        protected virtual MCTSNode BestUCTChild(MCTSNode node)
        {
            MCTSNode bestChild = null;
            double bestUCT = float.MinValue;
            double childUCT;
            foreach (MCTSNode childNode in node.ChildNodes){
                //calculate best UCT score
                if (childNode.N == 0)
                    //childUCT = float.PositiveInfinity;
                    return childNode; // an unvisited node's UCT is +infinity
                else
                    childUCT = (childNode.Q/childNode.N) + C * Math.Sqrt(Math.Log(node.N)/childNode.N);

                //compare with best
                if (childUCT > bestUCT){
                    bestChild = childNode;
                    bestUCT = childUCT;
                }
            }
            return bestChild;
        }

        //this method is very similar to the bestUCTChild, but it is used to return the final action of the MCTS search, and so we do not care about
        //the exploration factor
        protected MCTSNode BestChild(MCTSNode node)
        {
            float bestRatio = float.MinValue;
            MCTSNode bestNode = null;
            foreach (MCTSNode childNode in node.ChildNodes)
            {
                if (childNode.Q / childNode.N > bestRatio)
                {
                    bestRatio = childNode.Q / childNode.N;
                    bestNode = childNode;
                }
            }

            return bestNode;
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
