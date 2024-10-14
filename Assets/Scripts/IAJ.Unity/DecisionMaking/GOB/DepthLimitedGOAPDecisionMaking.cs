using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.IAJ.Unity.DecisionMaking.HeroActions;
using Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel;
using Assets.Scripts.Game;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.GOB
{
    public class DepthLimitedGOAPDecisionMaking
    {
        public const int MAX_DEPTH = 2;
        public int ActionCombinationsProcessedPerFrame { get; set; }
        public float TotalProcessingTime { get; set; }
        public float ProcessingTime { get; set; }
        public int TotalActionCombinationsProcessed { get; set; }
        public bool InProgress { get; set; }

        public WorldModel InitialWorldModel { get; set; }
        private List<Goal> Goals { get; set; }
        private WorldModel[] Models { get; set; }
        private Action[] LevelAction { get; set; }
        public Action[] BestActionSequence { get; private set; }
        public Action BestAction { get; private set; }
        public float BestDiscontentmentValue { get; private set; }
        private int CurrentDepth {  get; set; }

        public DepthLimitedGOAPDecisionMaking(WorldModel currentStateWorldModel, AutonomousCharacter character)
        {
            this.ActionCombinationsProcessedPerFrame = 2000;
            this.Goals = character.Goals;
            this.InitialWorldModel = currentStateWorldModel;
            this.TotalProcessingTime = 0.0f;
            this.TotalActionCombinationsProcessed = 0;
        }

        public void InitializeDecisionMakingProcess()
        {
            this.ProcessingTime = 0.0f;
            this.InProgress = true;
            this.CurrentDepth = 0;
            this.Models = new WorldModel[MAX_DEPTH + 1];
            this.Models[0] = this.InitialWorldModel;
            this.LevelAction = new Action[MAX_DEPTH];
            this.BestActionSequence = new Action[MAX_DEPTH];
            this.BestAction = null;
            this.BestDiscontentmentValue = float.MaxValue;
            this.InitialWorldModel.Initialize();
        }

        public Action ChooseAction()
        {
            var startTime = Time.realtimeSinceStartup;
            int actionCombinationsProcessedThisFrame = 0;

            float currentDiscontentment;
            Action nextAction;

            while (this.CurrentDepth >= 0)
            {
                if (CurrentDepth >= MAX_DEPTH)
                {
                    if (actionCombinationsProcessedThisFrame >= this.ActionCombinationsProcessedPerFrame)
                    {
                        this.TotalProcessingTime += Time.realtimeSinceStartup - startTime;
                        this.ProcessingTime += Time.realtimeSinceStartup - startTime;
                        return null;
                    }
                    currentDiscontentment = this.Models[CurrentDepth].Character.CalculateDiscontentment(this.Models[CurrentDepth]);
                    if (currentDiscontentment < BestDiscontentmentValue)
                    {
                        BestDiscontentmentValue = currentDiscontentment;
                        BestAction = this.LevelAction[0];
                        for (int i = 0; i < MAX_DEPTH; i++)
                        {
                            this.BestActionSequence[i] = this.LevelAction[i];
                        }
                    }
                    actionCombinationsProcessedThisFrame++;
                    TotalActionCombinationsProcessed++;
                    CurrentDepth--;
                    continue;
                }
                nextAction = this.Models[CurrentDepth].GetNextAction();
                if (nextAction != null)
                {
                    WorldModel nextWM = this.Models[CurrentDepth].GenerateChildWorldModel();
                    nextAction.ApplyActionEffects(nextWM);
                    if (nextWM.IsAlive()){
                        //Debug.Log("action found. can be executed...");
                        nextWM.Character.UpdateGoalsInsistence(nextWM);
                        this.Models[CurrentDepth + 1] = nextWM;
                        this.LevelAction[CurrentDepth] = nextAction;
                        CurrentDepth++;
                    }
                    else{
                        continue;
                    }
                }
                else
                {
                    CurrentDepth--;
                    //Debug.Log("decrease depth");
                }
            }

            this.TotalProcessingTime += Time.realtimeSinceStartup - startTime;
            this.ProcessingTime += Time.realtimeSinceStartup - startTime;
            this.InProgress = false;
            return this.BestAction;
        }
    }
}
