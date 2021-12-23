using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerManager : MonoBehaviour
{
    // Player name
    [HideInInspector] public string playerName { get; set; }

    // Player default speed
    private float playerDefaultSpeed = 3f;
    private float playerDefaultSwipeSpeed = .2f;
    [HideInInspector] public float playerSpeed { get; set; }
    [HideInInspector] public float playerSwipeSpeed { get; set; }

    // Position in arena (0 - 4)
    public int rowPos { get; set; }

    // Main control
    private GameManager manager;
    private Client network;
    private bool isDead = false;

    // UI 
    [SerializeField] private Canvas canvas;
    private Slider mySlider;
    private Transform finishPoint;
    private Vector2 startPos;
    [SerializeField] private Text nameText;
    private Text coinText;
    [SerializeField] private GameObject disconnectLogo;

    // Order in race
    public int PlayerOrder { get; private set; }
    [SerializeField] private Text orderText;

    // Animation
    private Animator animator;

    // Swipe control
    private Vector2 startTouchPos;
    private Vector2 endTouchPos;
    private Vector2 currentTouchPos;
    private bool touchStopped;
    private bool touchIsOn;
    private bool rowChanged;
    // Swipe control range
    private float swipeRange = 50;
    private float tabRange = 10;

    // Effect and skill --------------------------------------
    // Faster movement
    private bool fastEffectIsActive;
    private float fastEffectSpeed = 1.7f;
    private float defaultFastTime = 2f;
    private float fastCountDown;
    // Double Coin
    private bool isDoubleCoinActive;
    private float doubleCoinValue = 2f;
    private float defaultDoubleCoinTime = 5f;
    // Throw sandals
    private float defaultSandalsSlowTime = 2;
    // Shield
    private bool shieldIsActive;
    private float defaultShieldTime = 5f;
    // Invisible
    private bool isInvisible;
    private float defaultInvisibleTime = 3f;
    // Slow Movement
    private bool slowEffectIsActive;
    private float slowSpeed = .3f;
    private float defaultSlowTime = 1.5f;
    private float slowCountDown;

    // Level
    // Increase speed in certain position
    private float increaseSpeed = .5f;
    private float levelThreshold;

    // Network
    bool isDisconnect;

    // Audio
    private AudioManager audio;

    void Start()
    {
        // Game manager
        manager = FindObjectOfType<GameManager>();
        network = FindObjectOfType<Client>();
        finishPoint = manager.FinishPoint;

        audio = FindObjectOfType<AudioManager>();

        // Animation
        try
        {
            animator = GetComponent<Animator>();
            animator.SetFloat("Speed", 0);
        }
        catch
        {

        }

        // Set camera and UI
        if (playerName == network.MyName)
        {
            FindObjectOfType<CameraFollow>().playerPos = gameObject.transform;
            mySlider = manager.PlayerSlider;
            mySlider.value = 0;
            startPos = new Vector2(transform.position.x, transform.position.y);
            finishPoint = manager.FinishPoint;
            coinText = manager.coinText;
            coinText.text = GameDataLoader.TheData.Coin.ToString("n0");
        }
        // Set Name
        nameText.text = ExtractName(playerName);

        // Set UI order
        PlayerOrder = 1;
        orderText.text = PlayerOrder.ToString();
        // Set Camera
        canvas.worldCamera = FindObjectOfType<CameraFollow>().GetComponent<Camera>();

        // Set player speed
        playerSpeed = playerDefaultSpeed;

        // Set Start position
        transform.position = new Vector3(manager.rowXPos[rowPos], -2, 0);

        // Level
        levelThreshold = manager.LevelDistance;

        // Network 
        isDisconnect = false;
    }
    
    void Update()
    {
        // If the game is started
        if (manager.GameIsStarted && !isDisconnect && !isDead)
        {
            // Audio
            FindObjectOfType<AudioManager>().Play("Run");


            // Player start running
            transform.position = new Vector2(transform.position.x, transform.position.y + (playerSpeed * Time.deltaTime));

            // Animation
            try
            {
                animator.SetFloat("Speed", 1);
            }
            catch
            {

            }

            // Check changing row
            if (rowChanged)
            {
                // Send massage
                MovePositionRow();
            }

            // If this player is mine
            if (playerName == network.MyName)
            {
                // Detect swipe screen
                SwipeControl();

                // UI
                mySlider.value = (transform.position.y - startPos.y) / (finishPoint.transform.position.y - startPos.y);

                // Check if finish
                if (transform.position.y > finishPoint.position.y)
                {
                    // Audio
                    audio.Play("Finish");

                    // Some UI
                    manager.GameOver(true, PlayerOrder);
                }
            }

            // Increase character default speed each level
            if(transform.position.y > levelThreshold && !slowEffectIsActive)
            {
                levelThreshold += manager.LevelDistance;
                playerDefaultSpeed += increaseSpeed;
                if(!fastEffectIsActive && !slowEffectIsActive)
                {
                    playerSpeed = playerDefaultSpeed;
                }
            }
        }
    }

    // Start Sync position -------------------------------------------------------------------------------------------------
    public void BeginSyncPos()
    {
        StartCoroutine(SyncPos());
    }
    private IEnumerator SyncPos()
    {
        while (manager.GameIsStarted)
        {
            // Sync position
            string[] massage = new string[] { "SyncPlr", transform.position.x.ToString(), transform.position.y.ToString() };
            network.SendMassageClient("AllES", massage);

            yield return new WaitForSeconds(5f);
        }
    }
    // Sync Pos
    public void SyncPos(float x, float y)
    {
        if (Mathf.Abs(gameObject.transform.position.y - y) > .8f)
        {
            transform.position = new Vector2(x, y);
        }
    }

    // Player Control ------------------------------------------------------------------------------------------------------
    private void SwipeControl()
    {
        if (Input.GetMouseButtonDown(0))
        {
            startTouchPos = Input.mousePosition;
            touchIsOn = true;
        }
        if(touchIsOn)
        {
            currentTouchPos = Input.mousePosition;
            Vector2 distance = currentTouchPos - startTouchPos;
            
            if (!touchStopped)
            {
                if (distance.x < -swipeRange)
                {
                    if(rowPos != 0)
                    {
                        rowPos--;
                        string[] mas = new string[] { "ChangeRow", rowPos.ToString() };
                        network.SendMassageClient("All", mas);
                    }
                    touchStopped = true;
                }
                else if (distance.x > swipeRange)
                {
                    if (rowPos != 4)
                    {
                        rowPos++;
                        string[] mas = new string[] { "ChangeRow", rowPos.ToString() };
                        network.SendMassageClient("All", mas);
                    }
                    touchStopped = true;
                }
                else if (distance.y > swipeRange)
                {
                    Debug.Log("Up");
                    touchStopped = true;
                }
                else if (distance.y < -swipeRange)
                {
                    Debug.Log("Down");
                    touchStopped = true;
                }
            }
        }

        if(Input.GetMouseButtonUp(0))
        {
            touchIsOn = false;
            touchStopped = false;
            endTouchPos = Input.mousePosition;
            Vector2 distance = endTouchPos - startTouchPos;

            if(Mathf.Abs(distance.x) < tabRange && Mathf.Abs(distance.y) < tabRange)
            {
                Debug.Log("Tab");
            }
        }
    }
    private void MovePositionRow()
    {
        float beginPos = transform.position.x;
        Vector2 newPos = new Vector2(Mathf.Lerp(beginPos, manager.rowXPos[rowPos], .1f), transform.position.y);
        transform.position = newPos;

        if(transform.position.x == manager.rowXPos[rowPos])
        {
            rowChanged = false;
        }
    }
    public void SetBoolRowChange(int newRow)
    {
        rowPos = newRow;
        rowChanged = true;

        // Audio
        if(playerName == network.MyName)
        {
            audio.Play("PlayerSwipe");
        }
    }

    // Traps and Effect -----------------------------------------------------------------------------------------------------------
    public void SkillButton(int idChar)
    {
        if(idChar == 1)
        {
            // Bocil skill

        }
        else if (idChar == 2)
        {
            // Cewe kepang skill

        }
        else if (idChar == 3)
        {
            // Emak kos skill

        }
        else if (idChar == 4)
        {
            // Pak Ustad skill

        }
        else if (idChar == 5)
        {
            // Pocong skill

        }
    }
    // Fast Movement Effect
    public void FastMovement()
    {
        FastMovement(defaultFastTime);
    }
    public void FastMovement(float fastTime)
    {
        if (!isDead && !fastEffectIsActive)
        {
            if (playerName == network.MyName)
            {
                audio.Play("Fall");
            }
            fastEffectIsActive = true;
            playerSpeed = playerDefaultSpeed * fastEffectSpeed;
            playerSwipeSpeed = playerDefaultSwipeSpeed * fastEffectSpeed;
            fastCountDown = fastTime;
            StartCoroutine(FastMovementActive());
        }
        else if (!isDead && fastEffectIsActive)
        {
            fastCountDown = fastTime;
        }
    }
    private IEnumerator FastMovementActive()
    {
        // Return player speed after few second
        while(fastCountDown > 0)
        {
            float temp = fastCountDown;
            yield return new WaitForSeconds(fastCountDown);
            fastCountDown -= temp;

            if (fastCountDown <= 0)
            {
                if (!isDead && !slowEffectIsActive)
                {
                    playerSpeed = playerDefaultSpeed;
                    playerSwipeSpeed = playerDefaultSwipeSpeed;
                }
                fastEffectIsActive = false;
            }
        }
    }
    // Slow Movement Effect
    public void SlowMovement()
    {
        SlowMovement(defaultSlowTime);
    }
    public void SlowMovement(float slowTime)
    {
        if (!slowEffectIsActive && !isDead)
        {
            audio.Play("Fall");
            // Slowdown player
            slowEffectIsActive = true;

            playerSpeed = playerDefaultSpeed * slowSpeed;
            playerSwipeSpeed = playerDefaultSwipeSpeed * slowSpeed;
            slowCountDown = slowTime;
            StartCoroutine(SlowMovementActive());
        }
        else if (slowEffectIsActive && !isDead)
        {
            slowCountDown = slowTime;
        }
    }
    private IEnumerator SlowMovementActive()
    {
        // Return player speed after few second
        while(slowCountDown > 0)
        {
            float temp = slowCountDown;
            yield return new WaitForSeconds(slowCountDown);
            slowCountDown -= temp;

            if(slowCountDown <= 0)
            {
                if (!isDead && !fastEffectIsActive)
                {
                    playerSpeed = playerDefaultSpeed;
                    playerSwipeSpeed = playerDefaultSwipeSpeed;
                }
                slowEffectIsActive = false;
            }
        }
    }
    // Swimming animation
    public void IsSwimming(bool isTrue)
    {
        animator.SetBool("IsSwim", isTrue);

        StartCoroutine(SwimmingTime());
    }
    private IEnumerator SwimmingTime()
    {
        yield return new WaitForSeconds(5);
        animator.SetBool("IsSwim", false);
    }
    // Player Dead
    public void Dead(string obstacleName)
    {
        // Animation
        if(obstacleName == "Lava")
        {
            animator.SetTrigger("Burning");
        }
        else if (obstacleName == "Log" || obstacleName == "Ball")
        {
            animator.SetTrigger("Dead");
        }

        string[] a = { "PlayerDead", transform.position.x.ToString(), transform.position.y.ToString() };
        network.SendMassageClient("AllES", a);
        DeadMethod();
    }
    public void DeadMethod()
    {
        if (!isDead)
        {
            // Audio
            audio.Stop("PlayBGM");
            audio.Stop("Run");

            int audioRand = Random.Range(0, 3);
            if (audioRand == 0)
            {
                audio.Stop("Scream");
            }
            else if(audioRand == 1)
            {
                audio.Stop("DeadF");
            }
            else
            {
                audio.Stop("DeadM");
            }

            isDead = true;
            // Player dead
            if (playerName == network.MyName)
            {
                manager.GameOver(false, 0);
                playerSpeed = 0f;
                if (animator != null)
                    animator.SetFloat("Speed", playerSpeed);
            }
            else
            {
                playerSpeed = 0;
                if (animator != null)
                    animator.SetFloat("Speed", playerSpeed);
            }
        }
    }
    // Player Get Coin
    public void GetCoin(int value)
    {
        // Audio
        if (playerName == network.MyName)
        {
            FindObjectOfType<AudioManager>().Play("Coin");
        }
            
        if (isDoubleCoinActive)
        {
            GameDataLoader.TheData.Coin += (value * 2);
            coinText.text = GameDataLoader.TheData.Coin.ToString("n0");
        }
        else
        {
            GameDataLoader.TheData.Coin += value;
            coinText.text = GameDataLoader.TheData.Coin.ToString("n0");
        }
    }
    // Player Disconnected
    public void Disconnected()
    {
        Color dark = new Color(80 / 255f, 80 / 255f, 80 / 255f);

        if (playerName == network.MyName)
        {

        }
        else
        {
            isDisconnect = true;
            gameObject.GetComponent<SpriteRenderer>().color = dark;
            disconnectLogo.SetActive(true);
            playerSpeed = 0;
        }
    }
    
    // UI -----------------------------------------------------------------------------------
    public void ChangePlayerOrder(int value)
    {
        PlayerOrder = value;
        orderText.text = value.ToString();
    }
    private string ExtractName(string name)
    {
        int nameLength = name.Length - 6;
        return name.Substring(0, nameLength);
    }
}
