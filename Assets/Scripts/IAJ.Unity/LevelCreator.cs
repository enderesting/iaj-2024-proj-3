/*
 * Created by Manuel Guimarães for Artificial Intelligence on Games classes of Universidade de Lisboa
*/
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.AI;

public class LevelCreator : MonoBehaviour
{
    //Grid configuration
    private int width;
    private int height;
    private float cellSize;

    [Header("Don't change anything but the gridName")]
    public string gridName;
    private string gridPath;
    string[,] textLines;

    [Header("Prefabs")]
    public GameObject orcPrefab;
    public GameObject chestPrefab;
    public GameObject manaPotion;
    public GameObject healthPotion;
    public GameObject skeletonPrefab;
    public GameObject dragonPrefab;
    public GameObject wallPrefab;

    [Header("Spawn Conglomerates")]
    public Transform wallSpawn;
    public Transform monsterSpawn;
    public Transform chestSpawn;
    public Transform potionSpawn;


    // Create the grid according to the text file set in the "Assets/Resources/grid.txt"
    public  void GridMapVisual()
    {
        int manaPotionCounter = 0;
        int healthPotionCounter = 0;
        int orCounter = 0;
        int skullCounter = 0;
        int chestCounter = 0;


        int width = textLines.GetLength(0);
        int height = textLines.GetLength(1);

        //Informing the grid of nodes that are not walkable
        for (int i = 0; i < width; i++)
            for (int j = 0; j < height; j++) {

                // We are reading the textLines from the top left till the bottom right, we need to adjust accordingly
                var x = j;
                var y = height - i - 1;

                if (textLines[i, j] == "1")
                {
                    var wall = GameObject.Instantiate(wallPrefab, wallSpawn);
                    wall.transform.localPosition = new Vector3(x * cellSize, 1.0f, y * cellSize);
                    wall.transform.localScale *= cellSize;

                }

                else if (textLines[i, j] == "x")
                {
                    // Skeleton
                    var skel = GameObject.Instantiate(skeletonPrefab, monsterSpawn);
                    skel.transform.localPosition = new Vector3(x * cellSize, 0.0f, y * cellSize);
                    skel.transform.localScale *= cellSize;
                    skel.name = "Skelleton" + skullCounter;
                    skullCounter++;

                }

                else if (textLines[i, j] == "D")
                {
                    //Dragon
                    var drag = GameObject.Instantiate(dragonPrefab, monsterSpawn);
                    drag.transform.localPosition = new Vector3(x * cellSize, 0.0f, y * cellSize);
                    drag.transform.localScale *= cellSize * 8;
                    drag.name = "Mighty Dragon";

                }

                else if (textLines[i, j] == "o")
                {
                    //Orcs
                    var orc = GameObject.Instantiate(orcPrefab, monsterSpawn);
                    orc.transform.localPosition = new Vector3(x * cellSize, 0f, y * cellSize - 0.5f);
                    orc.transform.localScale *= cellSize;
                    orc.name = "Orc" + orCounter;
                    orCounter++;

                }

                else if (textLines[i, j] == "h")
                {
                    // H for Health
                    var health = GameObject.Instantiate(healthPotion, potionSpawn);
                    health.transform.localPosition = new Vector3(x * cellSize, 0f, y * cellSize);
                    health.transform.localScale *= cellSize * 1.5f;
                    health.name = "HealthPotion" + healthPotionCounter;
                    healthPotionCounter++;

                }

                else if (textLines[i, j] == "m")
                {
                    // M for Mana
                    var mana = GameObject.Instantiate(manaPotion, potionSpawn);
                    mana.transform.localPosition = new Vector3(x * cellSize, 0f, y * cellSize);
                    mana.transform.localScale *= cellSize * 1.5f ;
                    mana.name = "ManaPotion" + manaPotionCounter;
                    manaPotionCounter++;
                }

                else if (textLines[i, j] == "c")
                {
                    // C for Chest
                    var chest = GameObject.Instantiate(chestPrefab, chestSpawn);
                    chest.transform.localPosition = new Vector3(x * cellSize, 0.0f, y * cellSize);
                    chest.transform.localScale *= cellSize;
                    chest.name = "Chest" + chestCounter;
                    chestCounter++;

                } 
            }


      
    }

    public void CleanMap()
    {
        var wallNumber = wallSpawn.childCount;

        List<GameObject> toDelete = new List<GameObject>();

        for(int i = 0; i <wallNumber  ;i++)
        { 
           toDelete.Add(wallSpawn.GetChild(i).gameObject);
        }

        var monsterNumber = monsterSpawn.childCount;

        for (int i = 0; i < monsterNumber; i++)
        {
            toDelete.Add(monsterSpawn.GetChild(i).gameObject);
        }

        var potionNumber = potionSpawn.childCount;

        for (int i = 0; i < potionNumber; i++)
        {
            toDelete.Add(potionSpawn.GetChild(i).gameObject);
        }

        var chestNumber = chestSpawn.childCount;

        for (int i = 0; i < chestNumber; i++)
        {
            toDelete.Add(chestSpawn.GetChild(i).gameObject);
        }
        
        foreach(var d in toDelete)
        {
            DestroyImmediate(d);
        }

    }


    // Reading the text file that where the map "definition" is stored
    public void LoadGrid()
    {
        gridPath = "Assets/Resources/" + gridName + ".txt";

        //Read the text from directly from the test.txt file
        StreamReader reader = new StreamReader(gridPath);
        var fileContent = reader.ReadToEnd();
        reader.Close();
        var lines = fileContent.Split("\n"[0]);

        //Calculating Height and Width from text file
        height = lines.Length;
        width = lines[0].Length - 1;

        // CellSize Formula 
        cellSize = 100.0f / (width);

        textLines = new string[height, width];
        int i = 0;
        foreach (var l in lines)
        {
            var words = l.Split();
            var j = 0;

            var w = words[0];

            foreach (var letter in w)
            {
                textLines[i, j] = letter.ToString();
                j++;

                if (j == textLines.GetLength(1))
                    break;
            }

            i++;
            if (i == textLines.GetLength(0))
                break;
        }

    }

}
