using Assets.Scripts.IAJ.Unity.Utils;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Assets.Scripts.Game.NPCs;
using Assets.Scripts.IAJ.Unity.Formations;
using System;
using Object = UnityEngine.Object;
using UnityEngine.SceneManagement;
using Assets.Scripts.Game;
using static UnityEngine.EventSystems.EventTrigger;
using Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public static class GameConstants
    {
        public const float UPDATE_INTERVAL = 2.0f;
        public const int TIME_LIMIT = 150;
        public const int PICKUP_RANGE = 8;

    }
    //public fields, seen by Unity in Editor

    public AutonomousCharacter Character { get; set; }

    //[Header("UI Objects")]
    private Text HPText;
    private Text ShieldHPText;
    private Text ManaText;
    private Text TimeText;
    private Text XPText;
    private Text LevelText;
    private Text MoneyText;
    private Text DiaryText;
    private GameObject GameEnd;

    [Serializable]
    public enum MonsterControl
    {
        SleepingMonsters,
        BehaviourTreeMonsters,
        StateMachineMonsters
    }

    [Serializable]
    public enum FormationSettings
    {
        NoFormations,
        LineFormation,
        TriangleFormation
    }

    [Header("Enemy Settings")]
    [Tooltip("Here you choose what algorithms control the Monsters")]
    [SerializeField]
    public MonsterControl monsterControl;
    [Tooltip("Here you choose if formations are in use")]
    public FormationSettings formationSettings;

    [Header("To Increase the Challange")]
    public bool StochasticWorld = false;


    //fields
    public List<GameObject> chests { get; set; }
    public List<GameObject> skeletons { get; set; }
    public List<GameObject> orcs { get; set; }
    public List<GameObject> dragons { get; set; }
    public List<GameObject> enemies { get; set; }
    public List<GameObject> potions { get; set; }

    public List<FormationManager> Formations { get; set; }
    public Dictionary<string, GameObject> DisposableObjects { get; set; }
    public int InitialDisposableObjectsCount { get; set; }
    public bool WorldChanged { get; set; }

    private float nextUpdateTime = 0.0f;
    private float enemyAttackCooldown = 0.0f;
    public bool gameEnded { get; set; } = false;
    public Vector3 initialPosition { get; set; }

    //RL Properties
    public bool RLActive { get; set; }

    void Awake()
    {
        Instance = this;
        UpdateDisposableObjects();
        this.InitialDisposableObjectsCount = this.DisposableObjects.Count;
        this.WorldChanged = false;


        this.Character = GameObject.FindGameObjectWithTag("Player").GetComponent<AutonomousCharacter>();
        this.HPText = GameObject.Find("Health").GetComponent<Text>();
        this.XPText = GameObject.Find("XP:").GetComponent<Text>();
        this.ShieldHPText = GameObject.Find("Shield").GetComponent<Text>();
        this.LevelText = GameObject.Find("Level").GetComponent<Text>();
        this.TimeText = GameObject.Find("Time").GetComponent<Text>();
        this.ManaText = GameObject.Find("Mana").GetComponent<Text>(); ;
        this.MoneyText = GameObject.Find("Money").GetComponent<Text>();
        this.GameEnd = GameObject.Find("EndGame");
        this.GameEnd.GetComponent<RectTransform>().localScale *= 10;
        this.GameEnd.SetActive(false);


        this.initialPosition = this.Character.gameObject.transform.position;
    }

    public void UpdateDisposableObjects()
    {
        this.enemies = new List<GameObject>();
        this.DisposableObjects = new Dictionary<string, GameObject>();
        this.chests = GameObject.FindGameObjectsWithTag("Chest").ToList();
        this.potions = GameObject.FindGameObjectsWithTag("ManaPotion").ToList();
        this.potions.AddRange(GameObject.FindGameObjectsWithTag("HealthPotion").ToList());
        this.skeletons = GameObject.FindGameObjectsWithTag("Skeleton").ToList();
        this.orcs = GameObject.FindGameObjectsWithTag("Orc").ToList();
        this.dragons = GameObject.FindGameObjectsWithTag("Dragon").ToList();
        this.enemies.AddRange(this.skeletons);
        this.enemies.AddRange(this.orcs);
        this.enemies.AddRange(this.dragons);


        //adds all enemies to the disposable objects collection
        foreach (var enemy in this.enemies)
        {
            this.DisposableObjects.Add(enemy.name, enemy);
        }
        //add all chests to the disposable objects collection
        foreach (var chest in this.chests)
        {
           this.DisposableObjects.Add(chest.name, chest);
        }
        //adds all health and Mana potions to the disposable objects collection
        foreach (var potion in this.potions)
        {
            this.DisposableObjects.Add(potion.name, potion);
        }
    }

    private void Start()
    {
        this.RLActive = Character.TabularQLearningActive || Character.NNLearningActive;
    }

    void FixedUpdate()
    {
        if (!this.gameEnded)
        {
            if (Time.time > this.nextUpdateTime)
            {
                this.nextUpdateTime = Time.time + GameConstants.UPDATE_INTERVAL;
                this.Character.baseStats.Time += GameConstants.UPDATE_INTERVAL;
            }

            if (formationSettings != FormationSettings.NoFormations) 
                foreach (FormationManager formation in Formations) { formation.UpdateSlots(); }

            this.HPText.text = "HP: " + this.Character.baseStats.HP;
            this.XPText.text = "XP: " + this.Character.baseStats.XP;
            this.ShieldHPText.text = "Shield HP: " + this.Character.baseStats.ShieldHP;
            this.LevelText.text = "Level: " + this.Character.baseStats.Level;
            this.TimeText.text = "Time: " + this.Character.baseStats.Time;
            this.ManaText.text = "Mana: " + this.Character.baseStats.Mana;
            this.MoneyText.text = "Money: " + this.Character.baseStats.Money;

            if (this.Character.baseStats.HP <= 0 || this.Character.baseStats.Time >= GameConstants.TIME_LIMIT)
            {
                this.GameEnd.SetActive(true);
                this.gameEnded = true;
                this.GameEnd.GetComponentInChildren<Text>().text = "You Died";
                if (RLActive) { 
                    Character.Reward -= 100f;
                    if(Character.TabularQLearningActive)
                    {
                        Character.QLearning.goldLastEpisode = Character.baseStats.Money;
                        Character.QLearning.timeLastEpisode = Character.baseStats.Time;
                    }
                }
            }
            else if (this.Character.baseStats.Money >= 25)
            {
                this.GameEnd.SetActive(true);
                this.gameEnded = true;
                this.GameEnd.GetComponentInChildren<Text>().text = "Victory \n GG EZ";
                if (RLActive)
                { 
                    Character.Reward += 100f;
                    if (Character.TabularQLearningActive)
                    {
                        Character.QLearning.numberOfVictories++;
                        Character.QLearning.goldLastEpisode = Character.baseStats.Money;
                        Character.QLearning.timeLastEpisode = Character.baseStats.Time;
                    } 
                }
            }
        }
    }

    public void RestartGame()
    {
        this.GameEnd.SetActive(false);
        gameEnded = false;
        foreach (GameObject enemy in this.enemies)
        {
            enemy.SetActive(true);
            enemy.GetComponent<NPC>().Restart();
        }
        foreach (GameObject chest in this.chests)
        {
            chest.SetActive(true);
            chest.GetComponent<Consumable>().Reset();
        }
        foreach (GameObject potion in potions)
        {
            potion.SetActive(true);
            potion.GetComponent<Consumable>().Reset();
        }
        this.Character.Restart();
    }

 
    // Action effects on the Game World
    public void SwordAttack(GameObject enemy)
    {
        int damage = 0;

        Monster monster = enemy.GetComponent<Monster>();
        Monster.EnemyStats enemyData = enemy.GetComponent<Monster>().stats;
        
        if (enemy != null && enemy.activeSelf && InMeleeRange(enemy))
        {
            this.Character.AddToDiary(" I Sword Attacked " + enemy.name);

            if (this.StochasticWorld)
            {
                damage = enemy.GetComponent<Monster>().DmgRoll.Invoke();

                //attack roll = D20 + attack modifier. Using 7 as attack modifier (+4 str modifier, +3 proficiency bonus)
                int attackRoll = RandomHelper.RollD20() + 7;

                if (attackRoll >= enemyData.AC)
                {
                    //there was an hit, enemy is destroyed, gain xp
                    
                    // this.enemies.Remove(enemy);
                    this.DisposableObjects[enemy.name] = null;
                    enemy.SetActive(false);
                }
            }
            else
            {
                damage = enemyData.SimpleDamage;
                // this.enemies.Remove(enemy);
                this.DisposableObjects[enemy.name] = null;
                enemy.SetActive(false);
            }

            this.Character.baseStats.XP += enemyData.XPvalue;

            int remainingDamage = damage - this.Character.baseStats.ShieldHP;
            this.Character.baseStats.ShieldHP = Mathf.Max(0, this.Character.baseStats.ShieldHP - damage);

            if (remainingDamage > 0)
            {
                this.Character.baseStats.HP -= remainingDamage;
            }
            if (RLActive) { this.Character.Reward -= 6f; }

            this.WorldChanged = true;
        }
    }

    public void EnemyAttack(GameObject enemy)
    {
        if (Time.time > this.enemyAttackCooldown)
        {

            int damage = 0;

            Monster monster = enemy.GetComponent<Monster>();

            if (enemy.activeSelf && monster.InWeaponRange(GameObject.FindGameObjectWithTag("Player")))
            {

                this.Character.AddToDiary(" I was Attacked by " + enemy.name);
                this.enemyAttackCooldown = Time.time + GameConstants.UPDATE_INTERVAL;

                if (this.StochasticWorld)
                {
                    damage = monster.DmgRoll.Invoke();

                    //attack roll = D20 + attack modifier. Using 7 as attack modifier (+4 str modifier, +3 proficiency bonus)
                    int attackRoll = RandomHelper.RollD20() + 7;

                    if (attackRoll >= monster.stats.AC)
                    {
                        //there was an hit, enemy is destroyed, gain xp
                        //RemoveOrcFromFormation(monster);
                        // this.enemies.Remove(enemy);
                        this.DisposableObjects[enemy.name] = null;
                        enemy.SetActive(false);
                    }
                }
                else
                {
                    damage = monster.stats.SimpleDamage;
                    //RemoveOrcFromFormation(monster);
                    // this.enemies.Remove(enemy);
                    this.DisposableObjects[enemy.name] = null;
                    enemy.SetActive(false);
                }

                this.Character.baseStats.XP += monster.stats.XPvalue;

                int remainingDamage = damage - this.Character.baseStats.ShieldHP;
                this.Character.baseStats.ShieldHP = Mathf.Max(0, this.Character.baseStats.ShieldHP - damage);

                if (remainingDamage > 0)
                {
                    this.Character.baseStats.HP -= remainingDamage;
                    this.Character.AddToDiary(" I was wounded with " + remainingDamage + " damage");
                }

                this.WorldChanged = true;
            }
        }
    }

    public void DivineSmite(GameObject enemy)
    {
        if (enemy != null && enemy.activeSelf && InDivineSmiteRange(enemy) && this.Character.baseStats.Mana >= 2)
        {
            if (enemy.CompareTag("Skeleton"))
            {
                this.Character.baseStats.XP += 3;
                this.Character.AddToDiary(" I Smited " + enemy.name);
                // this.enemies.Remove(enemy);
                this.DisposableObjects[enemy.name] = null;
                enemy.SetActive(false);
                //Object.Destroy(enemy);
                this.Character.baseStats.Mana -= 2;
                this.WorldChanged = true;
                if (this.RLActive) { this.Character.Reward -= 3f; }
            }
        }
    }

    public void ShieldOfFaith()
    {
        if (this.Character.baseStats.Mana >= 5)
        {
            this.Character.baseStats.ShieldHP = 5;
            this.Character.baseStats.Mana -= 5;
            this.Character.AddToDiary(" My Shield of Faith will protect me!");
            if (this.RLActive) { this.Character.Reward -= 1; }
            this.WorldChanged = true;
        }
    }

    public void PickUpChest(GameObject chest)
    {

        if (chest != null && chest.activeSelf && InChestRange(chest))
        {
            this.Character.AddToDiary(" I opened  " + chest.name);
            // this.chests.Remove(chest);
            this.DisposableObjects[chest.name] = null;
            chest.SetActive(false);
            this.Character.baseStats.Money += 5;
            if (this.RLActive) this.Character.Reward += 6f;
            this.WorldChanged = true;
        }
    }


    public void GetManaPotion(GameObject manaPotion)
    {
        if (manaPotion != null && manaPotion.activeSelf && InPotionRange(manaPotion))
        {
            this.Character.AddToDiary(" I drank " + manaPotion.name);
            this.DisposableObjects[manaPotion.name] = null;
            manaPotion.SetActive(false);
            this.Character.baseStats.Mana = 10;
            if (this.RLActive) this.Character.Reward -= 1.0f;
            this.WorldChanged = true;
        }
    }

    public void GetHealthPotion(GameObject potion)
    {
        if (potion != null && potion.activeSelf && InPotionRange(potion))
        {
            this.Character.AddToDiary(" I drank " + potion.name);
            this.DisposableObjects[potion.name] = null;
            potion.SetActive(false);
            if (this.RLActive) {
                int recoveredHealth = Character.baseStats.MaxHP - Character.baseStats.HP;
                if (recoveredHealth <= 5)
                {
                    this.Character.Reward -= 5.0f; 
                }
                else
                {
                    this.Character.Reward -= 1.0f;
                }
            }
            this.Character.baseStats.HP = this.Character.baseStats.MaxHP;
            this.WorldChanged = true;
        }
    }

    public void LevelUp()
    {
        if (this.Character.baseStats.Level >= 4) return;

        if (this.Character.baseStats.XP >= this.Character.baseStats.Level * 10)
        {
            if (!this.Character.LevelingUp)
            {
                this.Character.AddToDiary(" I am trying to level up...");
                this.Character.LevelingUp = true;
                this.Character.StopTime = Time.time + AutonomousCharacter.LEVELING_INTERVAL;
            }
            else if (this.Character.StopTime < Time.time)
            { 
                this.Character.baseStats.Level++;
                this.Character.baseStats.MaxHP += 10;
                this.Character.baseStats.XP = 0;
                this.Character.AddToDiary(" I leveled up to level " + this.Character.baseStats.Level);
                this.Character.LevelingUp = false;
                if (this.RLActive) this.Character.Reward += GameConstants.TIME_LIMIT * 4 / Character.baseStats.Time;
                this.WorldChanged = true;
            }
        }
    }

    public void LayOnHands()
    {
        if (this.Character.baseStats.Level >= 2 && this.Character.baseStats.Mana >= 7)
        {
            this.Character.AddToDiary(" With my Mana I Lay Hands and recovered all my health.");
            if (this.RLActive) { this.Character.Reward -= 1.0f; }
            this.Character.baseStats.HP = this.Character.baseStats.MaxHP;
            this.Character.baseStats.Mana -= 7;
            this.WorldChanged = true;
        }
    }

    public void DivineWrath()
    {
        if (this.Character.baseStats.Level >= 3 && this.Character.baseStats.Mana >= 10)
        {
            //kill all enemies in the map
            foreach (var enemy in this.enemies)
            {
                this.Character.baseStats.XP += enemy.GetComponent<Monster>().stats.XPvalue;
                this.Character.AddToDiary(" I used the Divine Wrath and all monsters were killed! \nSo ends a day's work...");
                enemy.SetActive(false);
                this.DisposableObjects[enemy.name] = null;
                Object.Destroy(enemy);
            }

            enemies.Clear();
            this.WorldChanged = true;
        }
    }

    public void Rest()
    {
        if (!this.Character.Resting)
        {
            this.Character.AddToDiary(" I am resting");
            this.Character.Resting = true;
            this.Character.StopTime = Time.time + AutonomousCharacter.RESTING_INTERVAL;
        }
        else if (this.Character.StopTime < Time.time)
        {
            this.Character.baseStats.HP += AutonomousCharacter.REST_HP_RECOVERY;
            this.Character.baseStats.HP = Mathf.Min(this.Character.baseStats.HP, this.Character.baseStats.MaxHP);
            this.Character.Resting = false;
            this.WorldChanged = true;
        }
    }

    public void Teleport()
    {
        if (this.Character.baseStats.Level >= 2 && this.Character.baseStats.Mana >= 5)
        {
            this.Character.AddToDiary(" With my Mana I teleported away from danger.");
            this.Character.navMeshAgent.Warp(this.initialPosition);
            this.Character.baseStats.Mana -= 5;
            if (this.RLActive) { this.Character.Reward -= 1.0f; }
            this.WorldChanged = true;
        }

    }


    private bool CheckRange(GameObject obj, float maximumDistance)
    {
        var distance = (obj.transform.position - this.Character.gameObject.transform.position).sqrMagnitude;
        return distance <= maximumDistance;
    }


    public bool InMeleeRange(GameObject enemy)
    {
        return this.CheckRange(enemy, GameConstants.PICKUP_RANGE *2);
    }

    public bool InDivineSmiteRange(GameObject enemy)
    {
        return this.CheckRange(enemy, GameConstants.PICKUP_RANGE * 10);
    }

    public bool InChestRange(GameObject chest)
    {
        return this.CheckRange(chest, GameConstants.PICKUP_RANGE);
    }

    public bool InPotionRange(GameObject potion)
    {
        return this.CheckRange(potion, GameConstants.PICKUP_RANGE);
    }
}
