using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


// Class to control preparation of the games

public class StartPanel : MonoBehaviour
{
    // UI
    [SerializeField] private GameObject waitingPlayerPanel;
    [SerializeField] private GameObject startManualButton;
    [SerializeField] private GameObject roomName;
    [SerializeField] private Text playerInRoom;

    // Waiting time
    private float PlayerWaitingTime = 5f;
    private bool waitIsDone;

    // Count down time
    [SerializeField] private Text startText;
    private float countDownTime = 6f;
    private float startTime;

    // Minimal players in room
    private int minPlayerInRoom = 2;

    // Game manager
    private GameManager manager;
    private Client network;

    // Static room name
    public static string RoomName;

    private void Start()
    {
        startTime = Time.time;
        waitIsDone = false;

        manager = FindObjectOfType<GameManager>();
        network = FindObjectOfType<Client>();

        startManualButton.SetActive(false);

        if(RoomName != null)
        {
            roomName.transform.GetChild(0).GetComponent<Text>().text = RoomName;
        }
        else
        {
            roomName.SetActive(false);
        }

        StartCoroutine(CheckRoom());
    }

    // Update is called once per frame
    private void Update()
    {
        if(Time.time - startTime >= PlayerWaitingTime)
        {
            waitIsDone = true;
        }
    }

    // Check room condition
    IEnumerator CheckRoom()
    {
        while (!manager.GameIsStarted)
        {
            // Check room condition
            // network.PlayerCountInRoom() >= minPlayerInRoom
            if (network.isMaster && waitIsDone)
            {
                try
                {
                    startManualButton.SetActive(true);
                }
                catch
                {

                }
            }

            // Set UI players in room
            if(network.PlayerCountInRoom() <= 0)
            {
                playerInRoom.text = "1";
            }
            else
            {
                playerInRoom.text = network.PlayerCountInRoom().ToString();
            }
            
            yield return new WaitForSeconds(2);
        }
    }

    // For manually start the game
    public void StartGameServer()
    {
        // Send massage to all player that the game is started
        network.SendMassageClient("All", "StartGame");
    }

    // Start the games
    public void StartGame()
    {
        // Start the games
        manager.StartSpawning();

        // Set Panel
        Destroy(waitingPlayerPanel);

        // Begin count down
        StartCoroutine(CountDownStart());
    }

    private IEnumerator CountDownStart()
    {
        // Start count down
        while(countDownTime > 0)
        {
            if(countDownTime - 1 == 0)
            {
                startText.text = "RUN!";
            }
            else
            {
                startText.text = (((int)countDownTime) - 1).ToString();
            }

            yield return new WaitForSeconds(1);

            countDownTime--;
        }

        // Set Bool
        manager.GameIsStarted = true;

        // Begin Couretine
        manager.FindPlayers();
        network.StartSyncPlayer();

        Destroy(gameObject);
    }

}
