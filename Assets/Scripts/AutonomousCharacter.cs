using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;
using Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel;
using Assets.Scripts.IAJ.Unity.DecisionMaking.GOB;
using Assets.Scripts.IAJ.Unity.DecisionMaking.MCTS;
using Assets.Scripts.IAJ.Unity.DecisionMaking.HeroActions;
using Assets.Scripts.Game;
using Assets.Scripts.Game.NPCs;
using Assets.Scripts.IAJ.Unity.Utils;
using System;
using static GameManager;
using Action = Assets.Scripts.IAJ.Unity.DecisionMaking.HeroActions.Action;
using Assets.Scripts.IAJ.Unity.DecisionMaking.RL;
using System.IO;
using IAJ.Unity.DecisionMaking.RL;

public class AutonomousCharacter : NPC
{
    //constants
    public const string SURVIVE_GOAL = "Survive";
    public const string GAIN_LEVEL_GOAL = "GainLevel";
    public const string BE_QUICK_GOAL = "BeQuick";
    public const string GET_RICH_GOAL = "GetRich";

    public const float DECISION_MAKING_INTERVAL = 50.0f;
    public const float RESTING_INTERVAL = 5.0f;
    public const float LEVELING_INTERVAL = 10.0f;
    public const float ENEMY_NEAR_CHECK_INTERVAL = 0.5f;
    public const float ENEMY_DETECTION_RADIUS = 10.0f;
    public const int REST_HP_RECOVERY = 2;
    public const float SPEED = 8.0f;



    //UI Variables
    private Text SurviveGoalText;
    private Text GainXPGoalText;
    private Text BeQuickGoalText;
    private Text GetRichGoalText;
    private Text DiscontentmentText;
    private Text TotalProcessingTimeText;
    private Text BestDiscontentmentText;
    private Text ProcessedActionsText;
    private Text BestActionText;
    private Text BestActionSequence;
    private Text DiaryText;

    [Serializable]
    public enum CharacterControlType
    {
        ControlledByPlayer,
        GOB,
        GOAP,
        MCTS,
        MCTS_BiasedPlayout,
        TabularQLearning,
        NeuralNetwork
    }

    public enum RLOptions
    {
        TrainAndSave,
        LoadAndPlay,
        RetrainAndSave
    }

    [Serializable]
    public enum WorldModelSettings
    {
        DictionaryWorldModel,
        // FEARWorldModel,
    }

    [SerializeField] public WorldModelSettings worldModelSettings;

    [Header("Character Behaviour Algorithm")]
    [Tooltip("Here you choose what algorithm control Sir Uthgard")]
    [SerializeField]
    public CharacterControlType characterControl;

    [Header("Goal Weights")]
    public float SurviveGoalWeight = 0.0f;
    public float GainLevelGoalWeight = 0.0f;
    public float BeQuickGoalWeight = 0.0f;
    public float GetRichWeight = 0.0f;

    [Header("Goal Initial Insistences")]
    public float GainLevelInitialInsistence = 1;
    public float GetRichInitialInsistence = 25;

    public int MCTS_MaxIterations = 1000;
    public int MCTS_MaxIterationsPerFrame = 500;
    public int MCTS_MaxPlayoutDepth = 100;
    public int MCTS_NumberPlayouts = 1;

    [Header("RL Options")]
    public RLOptions RLLOptions;

    [Header("Decision Algorithm Options")]
    public bool ReactToEnemy = false;
 
    //[Header("Hero Actions")]
    public bool LevelUp = true;
    public bool GetHealthPotion = true;
    public bool SwordAttack = true;
    public bool GetManaPotion = true;
    public bool ShieldOfFaith = true;
    public bool DivineSmite = true;
    public bool Teleport = true;
    public bool LayOnHands = true;
    public bool Rest = false;



    public bool ControlledByPlayer { get; private set; }
    public bool GOBActive { get; private set; }
    public bool GOAPActive { get; private set; }
    public bool MCTSActive { get; private set; }
    public bool MCTSBiasedPlayoutActive { get; private set; }
    public bool TabularQLearningActive { get; private set; }
    public bool NNLearningActive { get; private set; }


    public Goal BeQuickGoal { get; private set; }
    public Goal SurviveGoal { get; private set; }
    public Goal GetRichGoal { get; private set; }
    public Goal GainLevelGoal { get; private set; }
    public List<Goal> Goals { get; set; }
    public List<Action> Actions { get; set; }
    public Action CurrentAction { get; private set; }
    public GOBDecisionMaking GOBDecisionMaking { get; set; }
    public DepthLimitedGOAPDecisionMaking GOAPDecisionMaking { get; set; }
    public MCTS MCTSDecisionMaking { get; set; } 
    public QLearning QLearning { get; set;} 
    public NeuralNetwork NeuralNetwork { get; set; }

    //public PolicyLearning PolicyLearning { get; set; }
    public GameObject NearEnemy { get; private set; }

    //For Reinforcement Learning
    public float Reward = 0f; // this gets reset every move
    public float RewardPerEpisode = 0f;
    public int MaxEpisodes = 100;
    public float LearningRate = 1f;
    public float LearningRateDecay = 0.99f;
    public float MinLearningRate = 0.1f;
    public float DiscountRate = 0.95f;
    public float ExploreRate = 1f;
    public float ExploreRateDecay = 0.95f;
    public float MinExploreRate = 0.01f;
    public int episodeCounter = 1;

    private List<float> episodeRewards = new();
    private List<float> episodeTimes = new();
    private List<int> episodeGolds = new();
    private List<int> episodeVictories = new();

    public float StopTime { get; set; }

    //Status fields for timing issues

    public bool Resting { get; set; }
    public bool LevelingUp { get; set; }

    //private fields for internal use only

    private float nextUpdateTime = 0.0f;
    private float lastUpdateTime = 0.0f;
    private float lastEnemyCheckTime = 0.0f;
    private float previousGold = 0.0f;
    private int previousLevel = 1;
    public TextMesh playerText;
    private GameObject closestObject;
    private string qtablePath;
    private string nnPath; // neural network path

    // Draw path settings
    private LineRenderer lineRenderer;

    // protected override void Awake()
    // {
    //     base.Awake();
    //     //Assign character control flags
    //     ControlledByPlayer = (characterControl == CharacterControlType.ControlledByPlayer);
    //     GOBActive = (characterControl == CharacterControlType.GOB);
    //     GOAPActive = (characterControl == CharacterControlType.GOAP);
    //     MCTSActive = (characterControl == CharacterControlType.MCTS);
    //     MCTSBiasedPlayoutActive = (characterControl == CharacterControlType.MCTS_BiasedPlayout);
    //     TabularQLearningActive = (characterControl == CharacterControlType.TabularQLearning);
    //     NNLearningActive = (characterControl == CharacterControlType.NeuralNetwork);
    // }

    public void Start()
    {
        //This is the actual speed of the agent
        lineRenderer = this.GetComponent<LineRenderer>();
        playerText.text = "";
        
        // define savePath
        qtablePath = Path.Combine(Application.persistentDataPath, "qtable.json");
        nnPath = Path.Combine(Application.persistentDataPath, "neuralnetwork.json");


        // Initializing UI Text
        this.BeQuickGoalText = GameObject.Find("BeQuickGoal").GetComponent<Text>();
        this.SurviveGoalText = GameObject.Find("SurviveGoal").GetComponent<Text>();
        this.GainXPGoalText = GameObject.Find("GainXP").GetComponent<Text>();
        this.GetRichGoalText = GameObject.Find("GetRichGoal").GetComponent<Text>();
        this.DiscontentmentText = GameObject.Find("Discontentment").GetComponent<Text>();
        this.TotalProcessingTimeText = GameObject.Find("ProcessTime").GetComponent<Text>();
        this.BestDiscontentmentText = GameObject.Find("BestDicont").GetComponent<Text>();
        this.ProcessedActionsText = GameObject.Find("ProcComb").GetComponent<Text>();
        this.BestActionText = GameObject.Find("BestAction").GetComponent<Text>();
        this.BestActionSequence = GameObject.Find("BestActionSequence").GetComponent<Text>();
        DiaryText = GameObject.Find("DiaryText").GetComponent<Text>();

        NearEnemy = null;

        //Assign character control flags
        ControlledByPlayer = (characterControl == CharacterControlType.ControlledByPlayer);
        GOBActive = (characterControl == CharacterControlType.GOB);
        GOAPActive = (characterControl == CharacterControlType.GOAP);
        MCTSActive = (characterControl == CharacterControlType.MCTS);
        MCTSBiasedPlayoutActive = (characterControl == CharacterControlType.MCTS_BiasedPlayout);
        TabularQLearningActive = characterControl == CharacterControlType.TabularQLearning;
        NNLearningActive = characterControl == CharacterControlType.NeuralNetwork;


        //initialization of the GOB decision making
        //let's start by creating 4 main goals

        this.SurviveGoal = new Goal(SURVIVE_GOAL, SurviveGoalWeight, 0.0f, this.baseStats.MaxHP);

        this.GainLevelGoal = new Goal(GAIN_LEVEL_GOAL, GainLevelGoalWeight, 1.0f, 4.0f)
        {
            InsistenceValue = GainLevelInitialInsistence, //Initial value
            ChangeRate = 0
        };

        this.GetRichGoal = new Goal(GET_RICH_GOAL, GetRichWeight, 0.0f, 25.0f)
        {
            InsistenceValue = GetRichInitialInsistence, //Initial value
            ChangeRate = 0
        };

        this.BeQuickGoal = new Goal(BE_QUICK_GOAL, BeQuickGoalWeight, 0.0f, (float)GameManager.GameConstants.TIME_LIMIT)
        {
            ChangeRate = 1
        };

        this.Goals = new List<Goal>
        {
            this.SurviveGoal,
            this.BeQuickGoal,
            this.GetRichGoal,
            this.GainLevelGoal
        };

        //initialize the available actions
        //Uncomment commented actions after you implement them

        this.Actions = new List<Action>();

        //First it is necessary to add the actions made available by the elements in the scene
        //Some actions might be off due to not yet being implemented, or for
        //debuging purposes

        foreach (var enemy in GameObject.FindGameObjectsWithTag("Skeleton"))
        {
            if (SwordAttack) { this.Actions.Add(new SwordAttack(this, enemy)); };

            if (DivineSmite) this.Actions.Add(new DivineSmite(this, enemy));
        }

        foreach (var enemy in GameObject.FindGameObjectsWithTag("Orc"))
        {
            if (SwordAttack) { this.Actions.Add(new SwordAttack(this, enemy)); };
        }

        foreach (var enemy in GameObject.FindGameObjectsWithTag("Dragon"))
        {
            if (SwordAttack) { this.Actions.Add(new SwordAttack(this, enemy)); };
        }

        foreach (var chest in GameObject.FindGameObjectsWithTag("Chest"))
        {
            //This action always needs to be active for the game to end with a victory
            this.Actions.Add(new PickUpChest(this, chest));
        }

        foreach (var potion in GameObject.FindGameObjectsWithTag("HealthPotion"))
        {
            if (GetHealthPotion) this.Actions.Add(new GetHealthPotion(this, potion));
        }

        foreach (var potion in GameObject.FindGameObjectsWithTag("ManaPotion"))
        {
            if (GetManaPotion) this.Actions.Add(new GetManaPotion(this, potion));
        }

        //Then we have a series of extra actions available to Sir Uthgard
        if (ShieldOfFaith) this.Actions.Add(new ShieldOfFaith(this));
        if (LevelUp) this.Actions.Add(new LevelUp(this));
        if (Teleport) this.Actions.Add(new Teleport(this));
        if (LayOnHands) this.Actions.Add(new LayOnHands(this));
        //if (Rest) this.Actions.Add(new Rest(this));


        // Initialization of Decision Making Algorithms
        if (!this.ControlledByPlayer)
        {
            if (this.GOBActive) this.GOBDecisionMaking = new GOBDecisionMaking(this.Actions, this.Goals);
            else if (this.GOAPActive)
            {
                // the WorldModel is necessary for the GOAP and MCTS algorithms that need to predict action effects on the world...
                WorldModel worldModel;

                // if (worldModelSettings == WorldModelSettings.FEARWorldModel)
                // {
                //     worldModel = new FEARWorldModel(GameManager.Instance, this, this.Actions, this.Goals);
                // }
                // else
                // {
                    worldModel = new DictionaryWorldModel(GameManager.Instance, this, this.Actions, this.Goals);
                // }
                    
                this.GOAPDecisionMaking = new DepthLimitedGOAPDecisionMaking(worldModel, this);
            }
            else if (this.MCTSActive)
            {
                WorldModel worldModel;
                
                // if (worldModelSettings == WorldModelSettings.FEARWorldModel)
                // {
                //     worldModel = new FEARWorldModel(GameManager.Instance, this, this.Actions, this.Goals);
                // }
                // else
                // {
                    worldModel = new DictionaryWorldModel(GameManager.Instance, this, this.Actions, this.Goals);
                // }
                
                this.MCTSDecisionMaking = new MCTS(worldModel, MCTS_MaxIterations, MCTS_MaxIterationsPerFrame, MCTS_NumberPlayouts, MCTS_MaxPlayoutDepth);
            }
            else if (this.MCTSBiasedPlayoutActive)
            {
                var WorldModel = new DictionaryWorldModel(GameManager.Instance, this, this.Actions, this.Goals);
                this.MCTSDecisionMaking = new MCTSBiased(WorldModel, MCTS_MaxIterations, MCTS_MaxIterationsPerFrame, MCTS_NumberPlayouts, MCTS_MaxPlayoutDepth, 1.0f);
            }
            else if (this.TabularQLearningActive)
            {
                if (RLLOptions == RLOptions.LoadAndPlay || RLLOptions == RLOptions.RetrainAndSave)
                {
                    string loadpath = Path.Combine(Application.persistentDataPath, "qtable.json");
                    this.QLearning = new QLearning(LearningRate, LearningRateDecay, MinLearningRate, DiscountRate, ExploreRate, ExploreRateDecay, MinExploreRate, this, loadpath);    
                }
                else 
                {
                this.QLearning = new QLearning(LearningRate, LearningRateDecay, MinLearningRate, DiscountRate, ExploreRate, ExploreRateDecay, MinExploreRate, this);
                }
            }
            else if (this.NNLearningActive)
            {
                int[] layers = { GameManager.Instance.DisposableObjects.Values.Count * 2 + 7, 16, 16, Actions.Count };
                if (RLLOptions == RLOptions.LoadAndPlay || RLLOptions == RLOptions.RetrainAndSave)
                {
                    string loadpath = Path.Combine(Application.persistentDataPath, "neuralnetwork.json");
                    this.NeuralNetwork =
                        new NeuralNetwork(this, layers, LearningRate, DiscountRate, NeuralNetwork.ActivationFunction.Sigmoid, true, loadpath);
                }
                else
                {
                    this.NeuralNetwork =
                        new NeuralNetwork(this, layers, LearningRate, DiscountRate, NeuralNetwork.ActivationFunction.Sigmoid, true);
                }
            }
        }

        DiaryText.text += "My Diary \n I awoke. What a wonderful day to kill Monsters! \n";
    }

    void FixedUpdate()
    {
        if (GameManager.Instance.gameEnded)
        {
            if(episodeCounter < MaxEpisodes)
            {
                if (TabularQLearningActive)
                {
                    if (this.RLLOptions != RLOptions.LoadAndPlay)
                    {
                        QLearning.UpdateQValue(Reward);
                        RewardPerEpisode += Reward;
                        AddToDiary(" Reward at the End of Episode: " + RewardPerEpisode);
                        episodeRewards.Add(RewardPerEpisode);
                        Reward = 0;
                        RewardPerEpisode = 0;
                        QLearning.UpdateParameters();

                        episodeTimes.Add(QLearning.timeLastEpisode);
                        episodeGolds.Add(QLearning.goldLastEpisode);
                        episodeVictories.Add(QLearning.numberOfVictories);

                        SaveEpisodeDataCSV();

                        Debug.Log("Save Brain to: " + qtablePath);
                        QLearning.tableQL.SaveQTable(qtablePath);
                    }

                    episodeCounter++;
                    GameManager.Instance.RestartGame();
                    AddToDiary(" Episode: " + episodeCounter);
                    Debug.Log("Episode: " + episodeCounter);

                    //Do here end-of-episode stuff
                    // string savePath = Path.Combine(Application.persistentDataPath, "qtable.json");

                    this.QLearning.InitializeQLearning();

                    return;
                } 
                else if (NNLearningActive)
                {
                    if (this.RLLOptions != RLOptions.LoadAndPlay)
                    {
                        NeuralNetwork.SetLastActionReward(Reward);
                        AddToDiary(" Reward: " + Reward);
                        episodeRewards.Add(RewardPerEpisode);
                        Reward = 0;
                        RewardPerEpisode = 0;
                        NeuralNetwork.TrainEpisode();

                        episodeTimes.Add(NeuralNetwork.timeLastEpisode);
                        episodeGolds.Add(NeuralNetwork.goldLastEpisode);
                        episodeVictories.Add(NeuralNetwork.numberOfVictories);
                        SaveEpisodeDataCSV();

                        Debug.Log("Save Brain to: " + nnPath);
                        NeuralNetwork.SaveBrain(nnPath);
                    }
                    
                    episodeCounter++;
                    GameManager.Instance.RestartGame();
                    AddToDiary(" Episode: " + episodeCounter);
                    Debug.Log("Episode: " + episodeCounter);
                    NeuralNetwork.InProgress = true;
                    return;
                }

            }
            Time.timeScale = 0; // Freeze the game
            //Do here end-of-training stuff
            return;
        }

        //Agent Perception 
        if (Time.time > this.lastEnemyCheckTime + ENEMY_NEAR_CHECK_INTERVAL)
        {
            GameObject enemy = CheckEnemies(ENEMY_DETECTION_RADIUS);
            if (enemy != null)
            {
                if (ReactToEnemy) GameManager.Instance.WorldChanged = true;
                // AddToDiary(" There is " + enemy.name + " in front of me!");
                this.NearEnemy = enemy;
            }
            else
            {
                this.NearEnemy = null;
            }
            this.lastEnemyCheckTime = Time.time;
        }

        if (Time.time > this.nextUpdateTime || GameManager.Instance.WorldChanged)
        {
            GameManager.Instance.WorldChanged = false;
            this.nextUpdateTime = Time.time + DECISION_MAKING_INTERVAL;

            //first step, perceptions
            //update the agent's goals based on the state of the world
            UpdateGoalsInsistence();

            //Write in the interface
            this.SurviveGoalText.text = "Survive: " + this.SurviveGoal.NormalizedInsistenceValue + " (" + this.SurviveGoal.Weight + ")";
            this.GainXPGoalText.text = "Gain Level: " + this.GainLevelGoal.NormalizedInsistenceValue.ToString("F1") + " (" + this.GainLevelGoal.Weight + ")";
            this.BeQuickGoalText.text = "Be Quick: " + this.BeQuickGoal.NormalizedInsistenceValue.ToString("F1") + " (" + this.BeQuickGoal.Weight + ")";
            this.GetRichGoalText.text = "GetRich: " + this.GetRichGoal.NormalizedInsistenceValue.ToString("F1") + " (" + this.GetRichGoal.Weight + ")";
            this.DiscontentmentText.text = "Discontentment: " + this.CalculateDiscontentment().ToString("F1");

            this.lastUpdateTime = Time.time;

            //To have a new decision lets initialize Decision Making Proccess
            this.CurrentAction = null;
            if (GOBActive)
            {
                this.GOBDecisionMaking.InProgress = true;
            }
            else if (GOAPActive)
            {
                this.GOAPDecisionMaking.InitializeDecisionMakingProcess();
            }
            else if (MCTSActive)
            {
                this.MCTSDecisionMaking.InitializeMCTSearch();
            }
            else if (MCTSBiasedPlayoutActive)
            {
                this.MCTSDecisionMaking.InitializeMCTSearch();
            }
            else if (TabularQLearningActive)
            {
                this.QLearning.InitializeQLearning();
            } else if (NNLearningActive)
            {
                this.NeuralNetwork.InProgress = true;
            }
        }

        if (this.ControlledByPlayer)
        {
            //Using the old Input System
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
                this.transform.position += new Vector3(0.0f, 0.0f, 0.1f) * SPEED;
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
                this.transform.position += new Vector3(0.0f, 0.0f, -0.1f) * SPEED;
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
                this.transform.position += new Vector3(-0.1f, 0.0f, 0.0f) * SPEED;
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
                this.transform.position += new Vector3(0.1f, 0.0f, 0.0f) * SPEED;
            if (Input.GetKey(KeyCode.F))
                if (closestObject != null)
                {
                    //Simple way of checking which object is closest to Sir Uthgard
                    var s = playerText.text.ToString();
                    if (s.Contains("Health Potion"))
                        PickUpHealthPotion();
                    else if (s.Contains("Mana Potion"))
                        PickUpManaPotion();
                    else if (s.Contains("Chest"))
                        PickUpChest();
                    else if (s.Contains("Enemy"))
                        AttackEnemy();
                }
        }

        else if (this.GOAPActive)
        {
            this.UpdateDLGOAP();
        }
        else if (this.GOBActive)
        {
            this.UpdateGOB();
        }
        else if (this.MCTSActive)
        {
            this.UpdateMCTS();
        }
        else if (this.MCTSBiasedPlayoutActive)
        {
            this.UpdateMCTS();
        }
        else if (this.TabularQLearningActive &&
        this.baseStats.HP > 0 && this.baseStats.Time < GameConstants.TIME_LIMIT && baseStats.Money < 25)
        {
            this.UpdateQLearning();
        }
        else if (this.NNLearningActive && this.baseStats.HP > 0 && this.baseStats.Time < GameConstants.TIME_LIMIT &&
                 baseStats.Money < 25)
        {
            this.UpdateNeuralNetwork();
        }
        //ToDo Update your RL algorithms here...

 

        if (this.CurrentAction != null)
        {
            if (this.CurrentAction.CanExecute())
            {
                this.CurrentAction.Execute();
            }
        }

        if (this.TabularQLearningActive && GameManager.Instance.WorldChanged && this.RLLOptions != RLOptions.LoadAndPlay &&
        this.baseStats.HP > 0 && this.baseStats.Time < GameConstants.TIME_LIMIT && baseStats.Money < 25)
        {
            QLearning.UpdateQValue(Reward);
            AddToDiary(" Reward after action: " + Reward);
            RewardPerEpisode += Reward;
            Reward = 0;
        }
        if (this.NNLearningActive && GameManager.Instance.WorldChanged && this.RLLOptions != RLOptions.LoadAndPlay &&
            this.baseStats.HP > 0 && this.baseStats.Time < GameConstants.TIME_LIMIT && baseStats.Money < 25)
        {
            NeuralNetwork.SetLastActionReward(Reward);
            AddToDiary(" Reward: " + Reward);
            Reward = 0;
        }

        if (navMeshAgent.hasPath)
        {
            DrawPath();
        }
    }

    private void UpdateGoalsInsistence()
    {
        var duration = Time.time - this.lastUpdateTime;
        // Max Health minus current Health
        this.SurviveGoal.Max = baseStats.MaxHP;
        this.SurviveGoal.InsistenceValue = baseStats.MaxHP - baseStats.HP;
 
        // How much time has passed...
        this.BeQuickGoal.InsistenceValue += this.BeQuickGoal.ChangeRate * duration;

        // This can go up as time passes, and then go down when the character gains a level
        this.GainLevelGoal.InsistenceValue += this.GainLevelGoal.ChangeRate * duration; //increase in goal over time
        if (baseStats.Level > this.previousLevel)
        {
            this.GainLevelGoal.InsistenceValue -= (baseStats.Level - this.previousLevel);
            this.previousLevel = baseStats.Level;
        }

        // This can start high or go up as time passes, but is reduced by how much treasure the character already got
        this.GetRichGoal.InsistenceValue += this.GetRichGoal.ChangeRate * duration;
        var goldDiff = baseStats.Money - this.previousGold;
        if (goldDiff > 0)
        {
            this.GetRichGoal.InsistenceValue -= goldDiff;
            this.previousGold = baseStats.Money;
        }
        
        // Normalize goals to 0-10
        foreach (Goal goal in Goals) { goal.NormalizeGoalValue(goal.InsistenceValue, goal.Min, goal.Max); }
    }

    //For use in GOAP...
    public void UpdateGoalsInsistence(WorldModel wM)
    {
        float duration = (float) wM.GetProperty(PropertiesName.DURATION);

        // Max Health minus current Health
        var surviveValue = (int)wM.GetProperty(PropertiesName.MAXHP) - (int)wM.GetProperty(PropertiesName.HP);
        wM.SetGoalValue(SURVIVE_GOAL, surviveValue);

        // How much time has passed...
        float beQuickValue = (float) wM.GetGoalValue(BE_QUICK_GOAL) + this.BeQuickGoal.ChangeRate * duration;
        wM.SetGoalValue(BE_QUICK_GOAL, beQuickValue);

        var gainLevelValue = wM.GetGoalValue(GAIN_LEVEL_GOAL) + this.GainLevelGoal.ChangeRate * duration;
        if ( (int)wM.GetProperty(PropertiesName.LEVEL) > (int)wM.GetProperty(PropertiesName.PreviousLEVEL) )
        {
            wM.SetGoalValue(GAIN_LEVEL_GOAL, gainLevelValue - 1);
            wM.SetProperty(PropertiesName.PreviousLEVEL, wM.GetProperty(PropertiesName.LEVEL));
        }

        // This can start high or go up as time passes, but is reduced by how much treasure the character already got
        var richGoalValue = wM.GetGoalValue(GET_RICH_GOAL) + this.GetRichGoal.ChangeRate * duration;
        var goldDiff = (int)wM.GetProperty(PropertiesName.MONEY) - (int)wM.GetProperty(PropertiesName.PreviousMONEY);
        if (goldDiff > 0)
        {
            wM.SetGoalValue(GET_RICH_GOAL, richGoalValue - goldDiff);
            wM.SetProperty(PropertiesName.PreviousMONEY, wM.GetProperty(PropertiesName.MONEY));
        }
        else
        {
            wM.SetGoalValue(GET_RICH_GOAL, richGoalValue);
        }
    }

    public float CalculateDiscontentment()
    {
        var discontentment = 0.0f;

        foreach (var goal in this.Goals)
        {
            discontentment += goal.GetDiscontentment();
        }
        return discontentment;
    }

    public float CalculateDiscontentment(WorldModel worldModel)
    {
        var discontentment = 0.0f;

        foreach (var goal in this.Goals)
        {
            var max = (goal.Name == SURVIVE_GOAL) ? (int)worldModel.GetProperty(PropertiesName.MAXHP) : goal.Max;
            var normalizedValue = goal.NormalizeGoalValue( worldModel.GetGoalValue(goal.Name), goal.Min, max);
            discontentment += goal.GetDiscontentment(normalizedValue);
        }
        return discontentment;
    }

    // Normalize different goal values to 0-10 ranges according to their max
 

    private GameObject CheckEnemies(float detectionRadius)
    {
        foreach (var enemy in GameManager.Instance.enemies)
        {
            Transform characterTransform = this.navMeshAgent.GetComponent<Transform>();
            Vector3 enemyDirection = enemy.GetComponent<Transform>().position - characterTransform.position;
            Vector3 characterDirection = navMeshAgent.velocity.normalized;
            //actually it checks if the enemy is in front of the character... it returns the first one...
            if (enemyDirection.sqrMagnitude < detectionRadius*detectionRadius &&
                Mathf.Cos(MathHelper.ConvertVectorToOrientation(characterDirection) - MathHelper.ConvertVectorToOrientation(enemyDirection.normalized)) > 0)
            {
                return enemy;
            }
        }
        return null;
    }

    public void AddToDiary(string s)
    {
        DiaryText.text += Time.time + s + "\n";

        if (DiaryText.text.Length > 600)
            DiaryText.text = DiaryText.text.Substring(500);
    }


    private void UpdateGOB()
    {

        bool newDecision = false;
        if (this.GOBDecisionMaking.InProgress)
        {
            //choose an action using the GOB Decision Making process
            var action = this.GOBDecisionMaking.ChooseAction(this);
            if (action != null && action != this.CurrentAction)
            {
                this.CurrentAction = action;
                newDecision = true;
                if (newDecision)
                {
                    var bestDiscont = this.GOBDecisionMaking.ActionDiscontentment[action];
                    Action secondBestAction = this.GOBDecisionMaking.secondBestAction;
                    var secondBestDiscont = secondBestAction != null ? this.GOBDecisionMaking.ActionDiscontentment[secondBestAction] : 0.0f;
                    Action thirdBestAction = this.GOBDecisionMaking.thirdBestAction;
                    var thirdBestDiscont = thirdBestAction != null ? this.GOBDecisionMaking.ActionDiscontentment[thirdBestAction] : 0.0f;
                    AddToDiary(" I decided to " + action.Name);
                    this.BestActionText.text = "Best Action:\n " + action.Name + ":" + bestDiscont.ToString("F2") + "\n";
                    this.BestActionSequence.text = " Second Best:\n" + (secondBestAction != null ? secondBestAction.Name : "null") + ":" + secondBestDiscont.ToString("F2") + "\n"
                        + " Third Best:\n" + (thirdBestAction != null ? thirdBestAction.Name : "null") + ":" + thirdBestDiscont.ToString("F2") + "\n";
                    
                    this.TotalProcessingTimeText.text = "Process. Time (ms): " + (this.GOBDecisionMaking.TotalProcessingTime * 1000).ToString("F") + " (" +
                                                        (this.GOBDecisionMaking.ProcessingTime * 1000).ToString("F") + ")";
                    this.BestDiscontentmentText.text = "Best Discontentment: " + this.GOBDecisionMaking.ActionDiscontentment[action].ToString("F");
                    this.ProcessedActionsText.text = "Act. comb. processed: " + this.GOBDecisionMaking.TotalActionCombinationsProcessed;
                }
            }
        }
    }

    private void UpdateDLGOAP()
    {
        bool newDecision = false;
        if (this.GOAPDecisionMaking.InProgress)
        {
            //choose an action using the GOB Decision Making process
            var action = this.GOAPDecisionMaking.ChooseAction();
            if (action != null && action != this.CurrentAction)
            {
                this.CurrentAction = action;
                newDecision = true;
            }
        }

        this.TotalProcessingTimeText.text = "Process. Time (ms): " + (this.GOAPDecisionMaking.TotalProcessingTime * 1000).ToString("F") + " (" +
                                            (this.GOAPDecisionMaking.ProcessingTime * 1000).ToString("F") + ")";
        this.BestDiscontentmentText.text = "Best Discontentment: " + this.GOAPDecisionMaking.BestDiscontentmentValue.ToString("F");
        this.ProcessedActionsText.text = "Act. comb. processed: " + this.GOAPDecisionMaking.TotalActionCombinationsProcessed;

        if (this.GOAPDecisionMaking.BestAction != null)
        {
            if (newDecision)
            {
                AddToDiary(" I decided to " + GOAPDecisionMaking.BestAction.Name);
            }
            var actionText = "";
            foreach (var action in this.GOAPDecisionMaking.BestActionSequence)
            {
                if (action != null) actionText += "\n" + action.Name;
            }
            this.BestActionSequence.text = "Best Action Sequence: " + actionText;
            this.BestActionText.text = "Best Action: " + GOAPDecisionMaking.BestAction.Name;
        }
        else
        {
            this.BestActionSequence.text = "Best Action Sequence:\nNone";
            this.BestActionText.text = "Best Action: \n Node";
        }
    }

    // You'll need this, when using MCTS
    private void UpdateMCTS()
    {
        if (MCTSDecisionMaking.InProgress)
        {
            var action = MCTSDecisionMaking.ChooseAction();
            if (action != null)
            {
                this.CurrentAction = action;
                AddToDiary(" I decided to " + action.Name);
            }
        }
        //Statistical and Debug Data
        this.TotalProcessingTimeText.text = "Process. Time (ms): " + (MCTSDecisionMaking.TotalProcessingTime * 1000).ToString("F") + " (" +
                                            (this.MCTSDecisionMaking.ProcessingTime * 1000).ToString("F") + ")";

        this.ProcessedActionsText.text = "Iterations: "
            + MCTSDecisionMaking.CurrentIterations.ToString()
            + "\n Max Sel Depth: "
            + MCTSDecisionMaking.MaxSelectionDepthReached.ToString()
            + "\n Max Playout Depth: "
            + MCTSDecisionMaking.MaxPlayoutDepthReached.ToString();

        if (MCTSDecisionMaking.BestFirstChild != null)
        {
            var q = MCTSDecisionMaking.BestFirstChild.Q / MCTSDecisionMaking.BestFirstChild.N;
            this.BestDiscontentmentText.text = "Best Exp. Q value: " + q.ToString("F05");
            var actionText = "";
 
            foreach (var node in MCTSDecisionMaking.BestSequence)
            {
                actionText += "\n" + node.Action.Name + " (" + node.Q + "/" + node.N + ")";
            }
            this.BestActionSequence.text = "Best Action Sequence: " + actionText;

            //What is the predicted state of the world?
            var endState = MCTSDecisionMaking.BestActionSequenceEndState; // previously BestActionSequenceWorldState
            var text = "";
            if (endState != null)
            {
                text += "Predicted World State:\n";
                text += "My Level:" + endState.GetProperty(PropertiesName.LEVEL) + "\n";
                text += "My HP:" + endState.GetProperty(PropertiesName.HP) + "\n";
                text += "My Money:" + endState.GetProperty(PropertiesName.MONEY) + "\n";
                text += "Time Passsed:" + endState.GetProperty(PropertiesName.TIME) + "\n";
                this.BestActionText.text = text;
                if ((int)endState.GetProperty(PropertiesName.HP) < 0 || (float)endState.GetProperty(PropertiesName.TIME) > 150)
                {
                    //stop here
                }
            }
            else this.BestActionText.text = "No EndState was found";
        }
        else
        {
            this.BestActionSequence.text = "Best Action Sequence:\nNone";
            this.BestActionText.text = "";
        }


    }

    private void UpdateQLearning()
    {
        if (this.QLearning.InProgress)
        {
            var action = this.QLearning.ChooseAction();
            if (action != null)
            {
                this.CurrentAction = action;
                AddToDiary(" I decided to " + action.Name);
            }
        }
        //Statistical and Debug Data
        this.TotalProcessingTimeText.text = "Episode #: " + episodeCounter + "\n";
        this.ProcessedActionsText.text = "Number of victories: " + QLearning.numberOfVictories + "\n";
        this.BestDiscontentmentText.text = "Gold last episode: " + QLearning.goldLastEpisode + "\n"
            + "Time last episode: " + QLearning.timeLastEpisode + "\n";
    }
    
    private void UpdateNeuralNetwork()
    {
        if (this.NeuralNetwork.InProgress)
        {
            var action = this.NeuralNetwork.ChooseAction();
            if (action != null)
            {
                this.CurrentAction = action;
                AddToDiary(" I decided to " + action.Name);
            }
        }
        //Statistical and Debug Data
        this.TotalProcessingTimeText.text = "Episode #: " + episodeCounter + "\n";
        this.ProcessedActionsText.text = "Number of victories: " + NeuralNetwork.numberOfVictories + "\n";
        this.BestDiscontentmentText.text = "Gold last episode: " + NeuralNetwork.goldLastEpisode + "\n"
                                           + "Time last episode: " + NeuralNetwork.timeLastEpisode + "\n";
    }


    public override void Restart()
    {
        if (TabularQLearningActive && (RLLOptions == RLOptions.TrainAndSave || RLLOptions == RLOptions.RetrainAndSave))
        {
            //ToDo 
        }
        base.Restart();
        LevelingUp = false;
        Resting = false;
    }




    void DrawPath()
    {
       
        lineRenderer.positionCount = navMeshAgent.path.corners.Length;
        lineRenderer.SetPosition(0, this.transform.position);

        if (navMeshAgent.path.corners.Length < 2)
        {
            return;
        }

        for (int i = 1; i < navMeshAgent.path.corners.Length; i++)
        {
            Vector3 pointPosition = new Vector3(navMeshAgent.path.corners[i].x, navMeshAgent.path.corners[i].y, navMeshAgent.path.corners[i].z);
            lineRenderer.SetPosition(i, pointPosition);
        }

    }

 

    //Player Controlled Character Stuff - Do not change

    //Functions designed for when the Player has control of the character
    void OnTriggerEnter(Collider col)
    {
        if (this.ControlledByPlayer)
        {
            if (col.gameObject.tag.ToString().Contains("HealthPotion"))
            {
                playerText.text = "Pickup Health Potion";
                closestObject = col.gameObject;
            }
            else if (col.gameObject.tag.ToString().Contains("ManaPotion"))
            {
                playerText.text = "Pickup Mana Potion";
                closestObject = col.gameObject;
            }
            else if (col.gameObject.tag.ToString().Contains("Chest"))
            {
                playerText.text = "Pickup Chest";
                closestObject = col.gameObject;
            }
            else if (col.gameObject.tag.ToString().Contains("Orc") || col.gameObject.tag.ToString().Contains("Skeleton"))
            {
                playerText.text = "Attack Enemy";
                closestObject = col.gameObject;
            }
        }
    }

    void OnTriggerExit(Collider col)
    {
        if (col.gameObject.tag.ToString() != "")
            playerText.text = "";
    }


    //Actions designed for when the Player has control of the character
    void PickUpHealthPotion()
    {
        if (closestObject != null)
            if (GameManager.Instance.InPotionRange(closestObject))
            {
                GameManager.Instance.GetHealthPotion(closestObject);
                closestObject = null;
                playerText.text = "";
            }
    }

    void PickUpManaPotion()
    {
        if (closestObject != null)
            if (GameManager.Instance.InPotionRange(closestObject))
            {
                GameManager.Instance.GetManaPotion(closestObject);
                closestObject = null;
                playerText.text = "";
            }
    }


    void PickUpChest()
    {
        if (closestObject != null)
            if (GameManager.Instance.InChestRange(closestObject))
            {
                GameManager.Instance.PickUpChest(closestObject);
                closestObject = null;
                playerText.text = "";
            }
    }

    void AttackEnemy()
    {
        if (closestObject != null)
            if (GameManager.Instance.InMeleeRange(closestObject))
            {
                GameManager.Instance.SwordAttack(closestObject);
                closestObject = null;
                playerText.text = "";
            }
    }

    private void SaveEpisodeDataCSV()
    {
        string savePath = Path.Combine(Application.persistentDataPath, "episodeData.csv");
        using (StreamWriter writer = new(savePath))
        {
            writer.WriteLine("Episode,EpisodeRewards,TimeAlive,GoldAccumulated,TotalVictories");
            for (int i = 0; i < episodeTimes.Count; i++)
            {
                writer.WriteLine($"{i + 1},{episodeRewards[i]},{episodeTimes[i]},{episodeGolds[i]},{episodeVictories[i]}");
            }
        }
        Debug.Log("Episode data saved to: " + savePath);
    }

}
