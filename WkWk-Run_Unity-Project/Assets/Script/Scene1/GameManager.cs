using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Analytics;

public class GameManager : MonoBehaviour
{
    // Panels
    //[SerializeField] private GameObject menuPanel;
    [SerializeField] private GameObject startPanel;
    [SerializeField] private GameObject waitingPlayerPanel;
    [SerializeField] private GameObject gameOverPanel;

    // Boolean
    public bool GameIsStarted { get; set; }
    public bool GameIsFinished { get; set; }
    public bool PlatformSpawningIsStarted { get; set; }

    // Screen size
    private float screenWidth;
    private float rowCount = 5;
    private float rowDist;

    // Player
    [SerializeField] public GameObject[] playerPrefab;
    [HideInInspector] public GameObject[] PlayerList { get; set; }

    // Platform Spawn Point
    [SerializeField] private Transform platformSpawnPoint;

    // Main platform spawn position
    private Vector3 platformSpawnPos;

    // Platform
    [SerializeField] private GameObject PlatformGround;
    // Obstacle
    public int ObstacleLevel { get; set; }
    private float LevelObstacleDistance;
    private int spawnObstacleGapLength;
    private float spawnObstacleYPosCount;
    private bool canSpawnObstacle;
    private bool canSendObstacleGap;
    [SerializeField] private GameObject[] candiPrefab; // ID = 1
    [SerializeField] private GameObject[] housePrefab; // ID = 2
    [SerializeField] private GameObject[] treePrefab;  // ID = 3
    [SerializeField] private GameObject[] wallPrefab;  // ID = 4
    [SerializeField] private GameObject[] waterPrefab; // ID = 5
    [SerializeField] private GameObject lavaPrefab;    // ID = 6
    // Booster
    [SerializeField] private GameObject coinPrefab;
    [SerializeField] private GameObject[] boosterPrefab;
    // Moving Obstacle
    [SerializeField] private GameObject[] movingObs;
    private int movingObsRandValue;

    // Scaling
    private float scaleFix;

    // Row position
    [SerializeField] private Transform[] rowPos;
    [HideInInspector] public float[] rowXPos { get; private set; }

    // Level value
    public float LevelDistance { get; private set; }

    // All Player position
    [HideInInspector] GameObject[] playersOrder;

    // Finish Position
    public Transform FinishPoint;
    public Slider PlayerSlider;
    public Image SliderImage;
    public Sprite[] SliderSpriteList;
    // UI
    public Text coinText;

    // Game Over Panel
    [SerializeField] private Image gameOverPanelSprite;
    [SerializeField] private Text gameOverPanelTitle;
    [SerializeField] private Text gameOverPanelCoin;
    [SerializeField] private Text gameOverPanelPlayerOrder;
    [SerializeField] public Sprite[] characterSpriteFace;

    // Network
    private Client network;

    // For analytics
    private int startCoin;

    // Start is called before the first frame update
    private void Start()
    {
        // Audio
        FindObjectOfType<AudioManager>().Play("PlayBGM");
        FindObjectOfType<AudioManager>().Stop("MenuBGM");

        // Networking
        network = FindObjectOfType<Client>();

        // Panels
        startPanel.SetActive(true);
        waitingPlayerPanel.SetActive(true);

        // Platform start pos
        platformSpawnPos = new Vector3(0, 0, 10);

        // Obstacle
        spawnObstacleGapLength = 0;
        spawnObstacleYPosCount = 0;
        canSpawnObstacle = true;
        canSendObstacleGap = false;
        movingObsRandValue = 35; // %

        // Set screen size
        Vector2 minPosCamera = Camera.main.ScreenToWorldPoint(new Vector2(0, 0));
        Vector2 maxPosCamera = Camera.main.ScreenToWorldPoint(new Vector2(Camera.main.pixelWidth, Camera.main.pixelHeight));

        // Screen width size
        screenWidth = maxPosCamera.x - minPosCamera.x;

        // Calculating row position
        rowDist = screenWidth / rowCount;
        rowXPos = new float[(int)rowCount];
        for(int i = 0; i < rowPos.Length; i++)
        {
            rowPos[i].position = new Vector3((minPosCamera.x + (rowDist * 0.5f)) + (rowDist * i), minPosCamera.y + (rowDist * 0.5f) - (rowDist * 5), 0);
            rowXPos[i] = rowPos[i].position.x;
        }

        // Calculating scale
        float width = Camera.main.orthographicSize * 2.0f * Screen.width / Screen.height;
        scaleFix = width / rowCount;

        // Game level
        LevelDistance = FinishPoint.position.y / 6;

        // Obstacle level
        ObstacleLevel = 0;
        LevelObstacleDistance = LevelDistance;

        // Spawn Player
        network.SendMassageClient("Server", "SpawnPlayer|" + GameDataLoader.TheData.selectedChar);
        // Set SLider Image
        SliderImage.sprite = SliderSpriteList[GameDataLoader.TheData.selectedChar];

        // Creating start map
        StartMapSpawn();

        // Analytics
        startCoin = GameDataLoader.TheData.Coin;
    }

    // Creating map for start
    private void StartMapSpawn()
    {
        while(platformSpawnPoint.position.y > rowPos[0].position.y)
        {
            // Create platform
            if (platformSpawnPos.y < platformSpawnPoint.position.y)
            {
                Instantiate(PlatformGround, platformSpawnPos, Quaternion.identity);
                platformSpawnPos = new Vector3(platformSpawnPos.x, platformSpawnPos.y + 10, platformSpawnPos.z);
            }

            for (int i = 0; i < rowPos.Length; i++)
            {
                // relocate row position
                rowPos[i].position = new Vector3(rowPos[i].position.x, rowPos[i].position.y + rowDist, rowPos[i].position.z);
            }
        }
    }

    // Platform spaner -------------------------------------------------------------------------------------------------
    public float spawnObstacleDelay = .3f;
    public void StartSpawning()
    {
        // Start spawning obstacle
        PlatformSpawningIsStarted = true;
        StartCoroutine(SpawnObstacle());
    }
    private IEnumerator SpawnObstacle()
    {
        while (PlatformSpawningIsStarted)
        {
            // Check spawn position and if this is master
            if (network.isMaster && FinishPoint.position.y + 33 > spawnObstacleYPosCount)
            {
                // Preparing massage data
                string[] massage = new string[(int)rowCount + 1];
                massage[0] = "SpawnObstacle";
                
                if(canSpawnObstacle)
                {
                    // Spawn here
                    if (ObstacleLevel == 0)
                    {
                        // Spawn Candi
                        int obsCount = Random.Range(1, 3);
                        if (obsCount == 1)
                        {
                            int ranPos = Random.Range(0, 5);
                            for(int i = 0; i < rowCount; i++)
                            {
                                if(i == ranPos)
                                {
                                    massage[i + 1] = "1";
                                }
                                else
                                {
                                    massage[i + 1] = "0";
                                }
                            }
                        }
                        else if (obsCount == 2)
                        {
                            for (int i = 0; i < rowCount; i++)
                            {
                                if (i == 0 || i == 4)
                                {
                                    massage[i + 1] = "1";
                                }
                                else
                                {
                                    massage[i + 1] = "0";
                                }
                            }
                        }

                        // Reset Gap
                        spawnObstacleGapLength = Random.Range(6, 13);
                    }
                    else if (ObstacleLevel == 1)
                    {
                        // Spawn Tree
                        int obsCount = Random.Range(1, 3);
                        if (obsCount == 1)
                        {
                            int ranPos = Random.Range(0, 5);
                            for (int i = 0; i < rowCount; i++)
                            {
                                if (i == ranPos)
                                {
                                    massage[i + 1] = "3";
                                }
                                else
                                {
                                    massage[i + 1] = "0";
                                }
                            }
                        }
                        else if (obsCount == 2)
                        {
                            for (int i = 0; i < rowCount; i++)
                            {
                                if (i == 0 || i == 4)
                                {
                                    massage[i + 1] = "3";
                                }
                                else
                                {
                                    massage[i + 1] = "0";
                                }
                            }
                        }

                        // Reset Gap
                        spawnObstacleGapLength = Random.Range(5, 11);
                    }
                    else if (ObstacleLevel == 2)
                    {
                        // Spawn House
                        int obsCount = Random.Range(1, 3);
                        if (obsCount == 1)
                        {
                            int ranPos = Random.Range(0, 5);
                            for (int i = 0; i < rowCount; i++)
                            {
                                if (i == ranPos)
                                {
                                    massage[i + 1] = "2";
                                }
                                else
                                {
                                    massage[i + 1] = "0";
                                }
                            }
                        }
                        else if (obsCount == 2)
                        {
                            for (int i = 0; i < rowCount; i++)
                            {
                                if (i == 0 || i == 4)
                                {
                                    massage[i + 1] = "2";
                                }
                                else
                                {
                                    massage[i + 1] = "0";
                                }
                            }
                        }

                        // Reset Gap
                        spawnObstacleGapLength = Random.Range(4, 10);
                    }
                    else if (ObstacleLevel == 3)
                    {
                        // Spawn Wall
                        int ranPos = Random.Range(0, 5);
                        for (int i = 0; i < rowCount; i++)
                        {
                            if (i == ranPos && ranPos != 2)
                            {
                                massage[i + 1] = "4";
                            }
                            else if (i == ranPos && ranPos == 2)
                            {
                                massage[1] = "4";
                            }
                            else
                            {
                                massage[i + 1] = "0";
                            }
                        }

                        // Reset Gap
                        spawnObstacleGapLength = Random.Range(5, 9);
                    }
                    else if (ObstacleLevel == 4)
                    {
                        // Spawn Water
                        int obsCount = Random.Range(1, 3);
                        if (obsCount == 1)
                        {
                            int ranPos = Random.Range(0, 5);
                            for (int i = 0; i < rowCount; i++)
                            {
                                if (i == ranPos)
                                {
                                    massage[i + 1] = "5";
                                }
                                else
                                {
                                    massage[i + 1] = "0";
                                }
                            }
                        }
                        else if (obsCount == 2)
                        {
                            for (int i = 0; i < rowCount; i++)
                            {
                                if (i == 0 || i == 4)
                                {
                                    massage[i + 1] = "5";
                                }
                                else
                                {
                                    massage[i + 1] = "0";
                                }
                            }
                        }

                        // Reset Gap
                        spawnObstacleGapLength = Random.Range(6, 13);
                    }
                    else if (ObstacleLevel == 5)
                    {
                        // Spawn Candi
                        int obsCount = Random.Range(1, 3);
                        if (obsCount == 1)
                        {
                            int ranPos = Random.Range(0, 5);
                            for (int i = 0; i < rowCount; i++)
                            {
                                if (i == ranPos)
                                {
                                    massage[i + 1] = "6";
                                }
                                else
                                {
                                    massage[i + 1] = "0";
                                }
                            }
                        }
                        else if (obsCount == 2)
                        {
                            for (int i = 0; i < rowCount; i++)
                            {
                                if (i == 0 || i == 4)
                                {
                                    massage[i + 1] = "6";
                                }
                                else
                                {
                                    massage[i + 1] = "0";
                                }
                            }
                        }

                        // Reset Gap
                        spawnObstacleGapLength = Random.Range(6, 13);
                    }

                    // Set bool
                    canSpawnObstacle = false;
                    canSendObstacleGap = true;

                    spawnObstacleYPosCount++;
                }
                else if(canSendObstacleGap)
                {
                    // Send Gap massage
                    network.SendMassageClient("All", "SpawnObstacleGap|" + spawnObstacleGapLength.ToString());
                    canSendObstacleGap = false;
                }
               
                // Send to other client if host
                network.SendMassageClient("All", massage);
            }

            // Spawn Coin
            SpawnCoin();

            // Always spawn platform ground
            if (FinishPoint.position.y + 33 > platformSpawnPos.y)
            {
                SpawnPlatformGround();
            }

            // Change obstacle
            if(spawnObstacleYPosCount > LevelObstacleDistance)
            {
                ObstacleLevel++;
                LevelObstacleDistance += LevelDistance;
            }

            yield return new WaitForSeconds(spawnObstacleDelay);
        }
    }
    public void SpawnObstacle(int[] platform)
    {
        // Spawn
        if(rowPos[0].position.y < FinishPoint.position.y)
        {

            for (int i = 0; i < rowPos.Length; i++)
            {
                // Instantiate new platform

                if (platform[i] == 1)
                {
                    // Spawn Candi
                    int randCandi = Random.Range(0, candiPrefab.Length);
                    Instantiate(candiPrefab[randCandi], new Vector3(rowPos[i].position.x, rowPos[i].position.y, 5), Quaternion.identity);
                }
                else if (platform[i] == 2)
                {
                    // Spawn House
                    int randHouse = Random.Range(0, housePrefab.Length);
                    Instantiate(housePrefab[randHouse], new Vector3(rowPos[i].position.x, rowPos[i].position.y, 5), Quaternion.identity);
                }
                else if (platform[i] == 3)
                {
                    // Spawn Tree
                    int randTree = Random.Range(0, treePrefab.Length);
                    Instantiate(treePrefab[randTree], new Vector3(rowPos[i].position.x, rowPos[i].position.y, 5), Quaternion.identity);
                }
                else if (platform[i] == 4)
                {
                    // Spawn Wall
                    int randWall = Random.Range(0, wallPrefab.Length);
                    Instantiate(wallPrefab[randWall], new Vector3(rowPos[i].position.x, rowPos[i].position.y, 5), Quaternion.identity);
                }
                else if (platform[i] == 5)
                {
                    // Spawn Water
                    int randWater = Random.Range(0, waterPrefab.Length);
                    Instantiate(waterPrefab[randWater], new Vector3(rowPos[i].position.x, rowPos[i].position.y, 5), Quaternion.identity);

                    // Randomize spawn moving obstacle
                    // Spawn Moving Obstacle
                    int randMvObs = Random.Range(1, 101);
                    if (randMvObs < movingObsRandValue)
                    {
                        string[] msg = { "SpawnMovingObs", Random.Range(0, movingObs.Length).ToString(), rowPos[i].position.x.ToString(), (rowPos[i].position.y + 5).ToString() };
                        network.SendMassageClient("All", msg);
                    }
                }
                else if (platform[i] == 6)
                {
                    // Spawn Lava
                    Instantiate(lavaPrefab, new Vector3(rowPos[i].position.x, rowPos[i].position.y, 5), Quaternion.identity);
                }

                // Relocate row position
                rowPos[i].position = new Vector3(rowPos[i].position.x, rowPos[i].position.y + rowDist, rowPos[i].position.z);
            }
        }
    }
    public void SpawnObstacleGap(int gapLength)
    {
        // Relocate Y Pos
        for (int i = 0; i < rowPos.Length; i++)
        {
            rowPos[i].position = new Vector3(rowPos[i].position.x, rowPos[i].position.y + (rowDist * gapLength), rowPos[i].position.z);
        }

        // Add y Pos
        spawnObstacleYPosCount += spawnObstacleGapLength;
        // Set Gap to 1
        spawnObstacleGapLength = 1;

        // Set bool
        canSpawnObstacle = true;
    }

    int coinCount = 0, coinRowCount = 0, delayRange;
    int[] coinRowSelected = new int[5];
    bool[] coinRowUsed = new bool[5];
    int coinYPos = 8;
    private void SpawnCoin()
    {      
        if (network.isMaster && FinishPoint.position.y > coinYPos)
        {
            if(coinCount > 0)
            {
                for(int i = 0; i < coinRowCount; i++)
                {
                    if(Random.Range(1, 101) < 5)
                    {
                        string[] m = { "SpawnBooster", Random.Range(0, boosterPrefab.Length).ToString(), rowXPos[coinRowSelected[i]].ToString(), coinYPos.ToString() };
                        network.SendMassageClient("All", m);
                    }
                    else
                    {
                        string[] n = { "SpawnCoin", rowXPos[coinRowSelected[i]].ToString(), coinYPos.ToString() };
                        network.SendMassageClient("All", n);
                    }
                }
                coinYPos++;
                coinCount--;
            }
            else
            {
                for (int i = 0; i < coinRowUsed.Length; i++)
                {
                    coinRowUsed[i] = false;
                }
                coinCount = Random.Range(5, 12);
                coinRowCount = Random.Range(1, 5);
                delayRange = Random.Range(5, 9);
                coinYPos += delayRange;
                for(int i = 0; i < coinRowCount; i++)
                {
                    coinRowSelected[i] = Random.Range(0, 5);
                    while (coinRowUsed[coinRowSelected[i]])
                    {
                        coinRowSelected[i] = Random.Range(0, 5);
                    }
                    coinRowUsed[coinRowSelected[i]] = true;
                }
            }
        }
        
    }
    public void SpawnCoin(float xPos, float yPos)
    {
        // Spawn
        GameObject temp = Instantiate(coinPrefab, new Vector3(xPos, yPos, 1), Quaternion.identity);
        // Re-scale
        temp.transform.localScale = new Vector3(scaleFix - .3f, scaleFix - .3f, scaleFix - .3f);
    }
    public void SpawnPlatformGround()
    {
        Instantiate(PlatformGround, platformSpawnPos, Quaternion.identity);
        platformSpawnPos = new Vector3(platformSpawnPos.x, platformSpawnPos.y + 10, platformSpawnPos.z);
    }
    public void SpawnBooster(int typeBoost, float xPos, float yPos)
    {
        // Spawn
        GameObject temp = Instantiate(boosterPrefab[typeBoost], new Vector3(xPos, yPos, 1), Quaternion.identity);
        // Re-scale
        temp.transform.localScale = new Vector3(scaleFix - .3f, scaleFix - .3f, scaleFix - .3f);
    }
    public void SpawnMovingObs(int typeObs, float xPos, float yPos)
    {
        // Spawn
        GameObject temp = Instantiate(movingObs[typeObs], new Vector3(xPos, yPos, 1), Quaternion.identity);

        // Debug
        Debug.Log("Spawn " + movingObs[typeObs].name);
    }

    // Player Order ----------------------------------------------------------------------------------------------------
    // Find all players
    public void FindPlayers()
    {
        PlayerManager[] temps = FindObjectsOfType<PlayerManager>();
        playersOrder = new GameObject[temps.Length];

        for (int i = 0; i < temps.Length; i++)
        {
            playersOrder[i] = temps[i].gameObject;
        }

        StartCoroutine(PlayerOrder());
    }
    // Determine player order
    private IEnumerator PlayerOrder()
    {
        while (!GameIsFinished)
        {
            // Sorting algorithm
            if (playersOrder.Length > 1)
            {
                for (int i = 0; i < playersOrder.Length - 1; i++)
                {
                    int min = i;
                    for (var j = i + 1; j < playersOrder.Length; j++)
                    {
                        if (playersOrder[min].transform.position.y < playersOrder[j].transform.position.y)
                        {
                            min = j;
                        }
                    }

                    if (min != i)
                    {
                        var temp = playersOrder[min];
                        playersOrder[min] = playersOrder[i];
                        playersOrder[i] = temp;
                        playersOrder[i].GetComponent<PlayerManager>().ChangePlayerOrder(i + 1);
                    }
                    else
                    {
                        playersOrder[i].GetComponent<PlayerManager>().ChangePlayerOrder(i + 1);
                    }

                    if (i == playersOrder.Length - 2)
                    {
                        playersOrder[i + 1].GetComponent<PlayerManager>().ChangePlayerOrder(i + 2);
                    }
                }
            }
            else
            {
                playersOrder[0].GetComponent<PlayerManager>().ChangePlayerOrder(1);
            }
            yield return new WaitForSeconds(.8f);
        }
    }

    // Game Over method -----------------------------------------------------------------------------------------------
    public void GameOver(bool isWin, int order)
    {
        StartCoroutine(OpenGameOverPanel(isWin, order));
    }
    private IEnumerator OpenGameOverPanel(bool isWin, int order)
    {
        GameIsFinished = true;
        yield return new WaitForSeconds(1.5f);
        if (isWin)
        {
            FindObjectOfType<AudioManager>().Play("Finish");
        }
        else
        {
            FindObjectOfType<AudioManager>().Play("Lose");
        }

        Time.timeScale = .3f;
        gameOverPanel.SetActive(true);
        gameOverPanelSprite.sprite = characterSpriteFace[GameDataLoader.TheData.selectedChar];
        gameOverPanelCoin.text = GameDataLoader.TheData.Coin.ToString("n0");
        SaveGame.SaveProgress(GameDataLoader.TheData);

        if (isWin && order == 1)
        {
            gameOverPanelTitle.text = "YOU WIN !!!";
            gameOverPanelPlayerOrder.text = "1st";
        }
        else if (isWin && order == 2)
        {
            gameOverPanelTitle.text = "NICE PLAY !";
            gameOverPanelPlayerOrder.text = "2nd";
        }
        else if (isWin && order == 3)
        {
            gameOverPanelTitle.text = "NOT BAD !";
            gameOverPanelPlayerOrder.text = "3rd";
        }
        else if (isWin && order == 4)
        {
            gameOverPanelTitle.text = "TRY AGAIN !";
            gameOverPanelPlayerOrder.text = "4th";
        }
        else if (isWin && order == 5)
        {
            gameOverPanelTitle.text = "TRY AGAIN !";
            gameOverPanelPlayerOrder.text = "5th";
        }
        else if (!isWin)
        {
            gameOverPanelTitle.text = "YOU LOSE !";
            gameOverPanelPlayerOrder.text = "0th";
        }

        // Analytic
        if (isWin)
        {
            Analytics.CustomEvent("Game Finish Win", new Dictionary<string, object> {
                { "Distance", network.myPlayer.transform.position.y + 2},
                { "Total Coin", GameDataLoader.TheData.Coin - startCoin},
                { "Character Selected", GameDataLoader.TheData.selectedChar}
            });
        }
        else
        {
            Analytics.CustomEvent("Game Finish Lose", new Dictionary<string, object> {
                { "Distance", network.myPlayer.transform.position.y + 2},
                { "Total Coin", GameDataLoader.TheData.Coin - startCoin},
                { "Character Selected", GameDataLoader.TheData.selectedChar}
            });
        }
    }

    // Exit Room
    public void ExitRoom()
    {
        network.SendMassageClient("Server", "ExitRoom");
    }
    public void OnExitRoom()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(0);
    }

    // Audio
    public void ButtonSFX()
    {
        FindObjectOfType<AudioManager>().Play("Button");
    }
}
