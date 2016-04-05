﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class LevelManager : MonoBehaviour {
    public GameObject playArea;
    public GameObject player;
    public GameObject building;
    public GameObject buildingTop;
    public GameObject peep;
    public Text scoreText;
    public Text livesText;
    public Text peepsText;
    public Image fuelIndicator;
    public Image countdownImage;

    public static LevelManager instance = null;

    private Vector3 levelDimensions;
    private int lives;
    private int maxLevelPeeps;
    private float maxBuildingHeight = 20f;
    private int score;

    private Vector3 areaOffset;
    private GameObject[,,] buildingBlocks;
    private bool paused;
    private int alivePeeps;
    private float countdownTimer = 0.02f;
    private bool countdownOn = true;
    private string nextScene;

    // Use this for initialization
    void Start () {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
        Setup();
    }

    void Setup()
    {
        InitLevelVars();
        InitPlayArea();
        ResetPlayer();
        CountdownStart();
    }

    void InitLevelVars()
    {
        levelDimensions = GameManager.instance.levelDimensions;
        lives = GameManager.instance.playerLives;
        maxLevelPeeps = GameManager.instance.levelPeeps;
        score = GameManager.instance.playerScore;
        nextScene = "";
    }

    void InitPlayArea()
    {
        areaOffset = new Vector3(levelDimensions.x / 10, 0f, levelDimensions.z / 10);
        playArea.transform.localScale = levelDimensions;
        InitBuildings();
        alivePeeps = maxLevelPeeps;
    }

    void ResetPlayer()
    {
        player.transform.position = new Vector3(0f, maxBuildingHeight + 5f, 0f);
        player.GetComponent<Player>().Revive();
    }

    void CountdownStart()
    {
        countdownOn = true;
        Time.timeScale = countdownTimer;
        countdownImage.sprite = Resources.Load("Materials/GUI/Countdown/Textures/Ready", typeof(Sprite)) as Sprite;
        countdownImage.enabled = true;
        Invoke("CountdownReady", countdownTimer * 2);
    }

    void CountdownReady()
    {
        Invoke("CountdownSteady", countdownTimer);
    }

    void CountdownSteady()
    {
        countdownImage.sprite = Resources.Load("Materials/GUI/Countdown/Textures/Steady", typeof(Sprite)) as Sprite;
        Invoke("CountdownGo", countdownTimer);
    }

    void CountdownGo()
    {
        countdownImage.sprite = Resources.Load("Materials/GUI/Countdown/Textures/Go", typeof(Sprite)) as Sprite;
        Invoke("CountdownEnd", countdownTimer);
    }

    void CountdownEnd()
    {
        countdownImage.enabled = false;
        countdownOn = false;
        Time.timeScale = 1f;
    }

    void InitBuildings()
    {
        float startX = (levelDimensions.x / 2) - (building.transform.localScale.x / 2) - areaOffset.x;
        float startY = building.transform.localScale.y / 2 + areaOffset.y;
        float startZ = (levelDimensions.z / 2) - (building.transform.localScale.z / 2) - areaOffset.z;
        Vector3 startPos = new Vector3(startX * -1, startY, startZ);

        Vector3 buildingLimits = new Vector3((levelDimensions.x - (areaOffset.x * 2)) / (building.transform.localScale.x * 2),
                                             maxBuildingHeight,
                                             (levelDimensions.z - (areaOffset.z * 2)) / (building.transform.localScale.z * 2));

        buildingBlocks = new GameObject[(int)buildingLimits.x, (int)buildingLimits.y, (int)buildingLimits.z];

        int[] peepLocations = CreatePeepLocations((int)buildingLimits.x * (int)buildingLimits.z, maxLevelPeeps);

        int currentLocation = 0;

        for (int x = 0; x < buildingLimits.x; x++)
        {
            for (int z = 0; z < buildingLimits.z; z++)
            {
                float buildingHeight = Random.Range(8f, buildingLimits.y);
                int buildingMaterialIndex = Random.Range(0, 8) + 1;
                int buildingRoofMaterialIndex = Random.Range(0, 8) + 1;
                Material buildingMaterial = Resources.Load("Materials/Buildings/tower" + buildingMaterialIndex, typeof(Material)) as Material;
                Material buildingTopMaterial = Resources.Load("Materials/Buildings/towertop" + buildingMaterialIndex, typeof(Material)) as Material;
                Material buildingRoofMaterial = Resources.Load("Materials/Buildings/roof" + buildingRoofMaterialIndex, typeof(Material)) as Material;

                for (int y = 0; y < buildingHeight; y++)
                {
                    Vector3 buildingPos = new Vector3(startPos.x + (x * (building.transform.localScale.x * 2)),
                                                      startPos.y + (y * building.transform.localScale.y),
                                                      startPos.z - (z * (building.transform.localScale.z * 2)));


                    GameObject buildingObject = new GameObject();

                    Vector3 coords = new Vector3(x, y, z);

                    if (y < (int)buildingHeight)
                    {
                        buildingObject = Instantiate(building, buildingPos, Quaternion.identity) as GameObject;
                        buildingObject.GetComponent<Renderer>().material = buildingMaterial;
                    } else
                    {
                        buildingObject = Instantiate(buildingTop, buildingPos + new Vector3(0f, -0.6f, 0f), Quaternion.identity) as GameObject;

                        buildingObject.transform.Find("SafeZone").GetComponent<Renderer>().material = buildingRoofMaterial;
                        for (int wall = 1; wall <= 4; wall++)
                        {
                            buildingObject.transform.Find("Wall" + wall).GetComponent<Renderer>().material = buildingTopMaterial;
                        }

                        if (System.Array.IndexOf(peepLocations, currentLocation) >= 0)
                        {
                            GameObject peep = InitPeep(buildingObject.transform.localPosition);
                            float peepRotation = Random.Range(0, 4);
                            peep.transform.Rotate(0f, 90 * peepRotation, 0f);
                            buildingObject.GetComponent<BuildingBlock>().peep = peep;
                        }
                    }

                    buildingObject.GetComponent<BuildingBlock>().coords = coords;
                    buildingObject.GetComponent<BuildingBlock>().buildingHeight = buildingHeight;
                    buildingBlocks[x, y, z] = buildingObject;
                }

                currentLocation++;
            }
        }
    }

    int[] CreatePeepLocations(int playArea, int maxPeeps)
    {
        if (maxPeeps >= playArea)
        {
            maxPeeps = playArea;
        }

        int[] locations = new int[playArea];

        for(int i = 0; i < playArea; i++)
        {
            locations[i] = i;
        }

        for (int i = locations.Length - 1; i > 0; i--)
        {
            int swapPoint = Random.Range(0, i);
            int temp = locations[i];
            locations[i] = locations[swapPoint];
            locations[swapPoint] = temp;
        }

        int[] positions = new int[maxPeeps];
        for (int i = 0; i < maxPeeps; i++)
        {
            positions[i] = locations[i];
        }
        return positions;
    }

    GameObject InitPeep(Vector3 position)
    {
        return Instantiate(peep, position + new Vector3(0f, 0.7f, 0f), Quaternion.identity) as GameObject;
    }

    void Update()
    {
        CheckPause();
        UpdateGui();
    }

    void CheckPause()
    {
        if (Input.GetButtonDown("Start"))
        {
            paused = !paused;
        }
        if (!countdownOn)
        {
            if (paused)
            {
                Time.timeScale = 0f;
            }
            else
            {
                Time.timeScale = 1f;
            }
        }
    }

    public void BlowUpBuildingAbove(Vector3 coords)
    {
        GameObject buildingObject = buildingBlocks[(int)coords.x, (int)coords.y + 1, (int)coords.z];
        buildingObject.GetComponent<BuildingBlock>().Destroy(false);
    }

    public void SetFireBuildingBelow(Vector3 coords)
    {
        GameObject buildingObject = buildingBlocks[(int)coords.x, (int)coords.y - 1, (int)coords.z];
        buildingObject.GetComponent<BuildingBlock>().OnFire();
    }

    public void PlayerDie()
    {
        UpdateLives();
        CheckLevelStatus();
        Invoke("ResetPlayer", 1f);
    }

    public void GotPeep(bool died)
    {
        if (died)
        {
            UpdateScore(10);
        } else
        {
            UpdateScore(50);
            AddPlayerFuel(10);
        }
        alivePeeps--;
        CheckLevelStatus();
    }

    void CheckLevelStatus()
    {
        if (lives <= 0)
        {
            LevelEnd("GameOver");
        }

        if (alivePeeps <= 0)
        {
            LevelEnd("NextLevel");
        }
    }

    void LevelEnd(string scene)
    {
        player.GetComponent<Player>().isAlive = false;
        nextScene = scene;
        Invoke("LoadNextScene", 0.5f);
    }

    void LoadNextScene()
    {
        string scene = nextScene;
        nextScene = "";
        GameManager.instance.LoadScene(scene);
    }

    void UpdateLives()
    {
        lives--;
        GameManager.instance.playerLives = lives;
    }

    void UpdateScore(int scoreToAdd)
    {
        score += scoreToAdd;
        GameManager.instance.playerScore = score;
    }

    void AddPlayerFuel(float fuelToAdd)
    {
        player.GetComponent<Player>().AddFuel(fuelToAdd);
    }

    void UpdateGui()
    {
        scoreText.text = score.ToString().PadLeft(11, '0');
        livesText.text = lives.ToString();
        peepsText.text = alivePeeps.ToString();

        float fuelLeft = player.GetComponent<Player>().fuel / player.GetComponent<Player>().defaultFuel;
        fuelIndicator.transform.localScale = new Vector3(fuelLeft, fuelIndicator.transform.localScale.y, fuelIndicator.transform.localScale.z);
    }
}
