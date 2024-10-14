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
    public class MCTSBiased : MCTS
    {
        public MCTSBiased(WorldModel currentStateWorldModel, int maxIter, int maxIterFrame, int playouts, int playoutDepthLimit, float biasFactor)
                    : base(currentStateWorldModel, maxIter, maxIterFrame, playouts, playoutDepthLimit)
        {
            //Debug.Log("this is the new constructor with biased");
        }

        protected override MCTSNode Selection(MCTSNode initialNode, ref int maxSelectionDepth)
        {
            // selecting from existing tree the best node based on an equation
            // expand: whichever best node it lands on, find an unexplored action and explore the node
            Action nextAction;
            MCTSNode currentNode = initialNode;

            while (!currentNode.State.IsTerminal())
            {
                // not fully explored
                if ((nextAction = currentNode.State.GetNextAction()) is not null)
                {
                    return Expand(currentNode, nextAction);
                }

                currentNode = BestUCTChild(currentNode);
            }

            return currentNode;
        }

        protected override float Playout(WorldModel initialStateForPlayout, ref int playoutCount)
        {
            WorldModel currentState = initialStateForPlayout;
            Action selectedAction = null;
            float reward = 0;
            int playoutDepth = 0;
            while (!currentState.IsTerminal() && playoutDepth < this.PlayoutDepthLimit
                    && playoutCount < NumberPlayouts)
            {
                Action[] actionsUnordered = currentState.GetExecutableActions();
                List<Action> executableActions = actionsUnordered.OrderBy( o => o.GetHValue(currentState)).ToList();
                /*foreach( Action act in executableActions){
                    Debug.Log(act.ToString() + " " + act.GetHValue(currentState));
                }*/
                WorldModel newState = currentState.GenerateChildWorldModel();

                // Debug.Log("selected action:" + selectedAction);
                selectedAction = executableActions[0];

                selectedAction.ApplyActionEffects(newState);
                newState.CalculateNextPlayer();
                currentState = newState;
                playoutDepth++;
                playoutCount++;
            }
            reward = currentState.GetScore();
            return reward;
        }

    }
}
