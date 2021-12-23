using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;

public class Client : MonoBehaviour
{
    private TcpClient client;
    private NetworkStream networkStream;
    private int port = 3002;
    public IPAddress ipAd = IPAddress.Parse("45.130.229.104");
    // 127.0.0.1
    // 45.130.229.104

    // Player and Room name

    public string MyName { get; private set; }
    public string MyRealName { get; set; }
    public string roomName { get; set; }

    // Connection status
    public bool isConnected { get; private set; }
    private bool isReady = false;

    // Check connection timer
    float CheckTime = 8;
    float checkCountDown;

    // Master of room
    [SerializeField] public bool isMaster;

    // All players (in room)
    private List<PlayerManager> playerListInRoom;
    public PlayerManager myPlayer { get; private set; }

    RsaEncryption rsaEncryption;
    AesEncryption aesEncryption;

    void Start()
    {
        // Never destroy this object
        DontDestroyOnLoad(gameObject);

        MyRealName = GameDataLoader.TheData.UserName;
        GenerateName();

        client = new TcpClient();
        checkCountDown = CheckTime;
        playerListInRoom = new List<PlayerManager>();

        rsaEncryption = new RsaEncryption();
        aesEncryption = new AesEncryption();
 
        StartCoroutine(TryConnecting());
    }

    // Generate Name + 6 Random number
    public void GenerateName()
    {
        MyName = MyRealName + UnityEngine.Random.Range(10, 100) + UnityEngine.Random.Range(10, 100) + UnityEngine.Random.Range(10, 100);
    }

    // Try connecting to server
    private IEnumerator TryConnecting()
    {
        int count = 0;
        while (!client.Connected)
        {
            // Wait 2 second and try agaian
            yield return new WaitForSeconds(2);

            count++;
            try
            {
                client.Connect(ipAd, port);
                networkStream = client.GetStream();
                PrepareEncryption();

                Debug.Log("Connected to server");
            }
            catch (Exception e)
            {
                Debug.Log("Try connecting-" + count + " error : " + e.Message);
            }
        }
    }

    void Update()
    {
        if (client.Connected && isReady)
        {
            if (networkStream.DataAvailable)
            {
                BinaryFormatter formatter = new BinaryFormatter();
                ReceiveMassage(formatter.Deserialize(networkStream) as string);
            }

            // Checking connection
            if (checkCountDown > 0)
            {
                checkCountDown -= Time.deltaTime;
                isConnected = true;
            }
            else
            {
                isConnected = false;
            }
        }
    }

    // Preparation ---------------------------------------------------------------------------------------------
    private void PrepareEncryption()
    {
        // Send client public key to server
        BinaryFormatter formatter = new BinaryFormatter();
        string key = rsaEncryption.ConvertKeyToString(rsaEncryption.clientPublicKey);
        string encrypKey = rsaEncryption.Encrypt(key, rsaEncryption.serverPublicKey);
        formatter.Serialize(networkStream, encrypKey);

        // Wait for new key
        string answer = formatter.Deserialize(networkStream) as string;
        string aesKey = rsaEncryption.Decrypt(answer, rsaEncryption.clientPrivateKey);

        // Save the new key
        aesEncryption.SetKey(aesEncryption.ConvertStringToKey(aesKey));

        // Wait other massage
        answer = formatter.Deserialize(networkStream) as string;
        string order = aesEncryption.Decrypt(answer);
        string[] data = order.Split('|');
        if(data[0] == "Server" && data[1] == "WHORU")
        {
            formatter.Serialize(networkStream, aesEncryption.Encrypt("Server|" + MyName));
        }

        // Ready to communicate
        isReady = true;
        isConnected = true;
    }

    // Receiving Massage ---------------------------------------------------------------------------------------
    private void ReceiveMassage(string massage)
    {
        // Decrypt
        // string massage = aesEncryption.Decrypt(massage);

        // Debugging
        //Debug.Log(massage);

        // receive format : Sender|Data1|Data2|...
        string[] data = massage.Split('|');
        if(data[0] == "Server")
        {
            switch (data[1]) 
            {
                case "WHORU":
                    // Send player name
                    //BinaryFormatter format = new BinaryFormatter();
                    //format.Serialize(networkStream, aesEncryption.Encrypt("Server|" + MyName));
                    //SendMassageClient("Server", MyName);
                    break;
                case "SYNA":
                    // Connection check success
                    checkCountDown = CheckTime;
                    break;
                case "CreatedRoom":
                    // If joined in room
                    FindObjectOfType<MainMenuManager>().OnJoinedRoom();
                    // If creating room, auto room owner (master)
                    isMaster = true;
                    break;
                case "JoinedRoom":
                    // If joined in room
                    FindObjectOfType<MainMenuManager>().OnJoinedRoom();
                    roomName = data[2];
                    break;
                case "RoomNotFound":
                    FindObjectOfType<JoinRoomPanel>().RoomNotFound();
                    break;
                case "SpawnPlayer":
                    // Spawn player
                    SpawnPlayer(data[2], int.Parse(data[3]), int.Parse(data[4]), StringToBool(data[5]));
                    break;
                case "SetToMaster":
                    isMaster = true;
                    break;
                case "ExitRoom":
                    // Reset data
                    playerListInRoom = new List<PlayerManager>();
                    isMaster = false;
                    myPlayer = null;
                    roomName = "";

                    // Reload scene menu
                    FindObjectOfType<GameManager>().OnExitRoom();
                    break;
                case "Disconnect":
                    foreach (PlayerManager a in playerListInRoom)
                    {
                        // Refresh player position
                        if (a.playerName == data[2])
                        {
                            // Do something to disconnect player
                            a.Disconnected();
                        }
                    }
                    break;
                default:
                    Debug.Log("Unreconized massage : " + massage);
                    break;
            }
        }
        else if(data[0] == "Client")
        {
            switch (data[1])
            {
                case "SpawnPlayer":
                    SpawnPlayer(data[2], int.Parse(data[3]), int.Parse(data[4]), StringToBool(data[5]));
                    break;
                case "SpawnObstacle":
                    int[] platformData = new int[] { int.Parse(data[2]), int.Parse(data[3]), int.Parse(data[4]), int.Parse(data[5]), int.Parse(data[6]), };
                    FindObjectOfType<GameManager>().SpawnObstacle(platformData);
                    try
                    {
                        for(int i = 0; i < 5; i++)
                        {
                            int ahfuwe = int.Parse(data[i + 2]);
                        }
                    }
                    catch
                    {
                        for (int i = 0; i < 5; i++)
                        {
                            Debug.Log(data[i + 2] + " ");
                        }
                    }
                    break;
                case "SpawnObstacleGap":
                    FindObjectOfType<GameManager>().SpawnObstacleGap(int.Parse(data[2]));
                    break;
                case "SpawnMovingObs":
                    FindObjectOfType<GameManager>().SpawnMovingObs(int.Parse(data[2]), float.Parse(data[3]), float.Parse(data[4]));
                    break;
                case "SyncPlr":
                    foreach(PlayerManager a in playerListInRoom)
                    {
                        // Refresh player position
                        if(a.playerName == data[2])
                        {
                            Debug.Log("Sync Player : " + data[2] + " " + float.Parse(data[3]) + " " + float.Parse(data[4]));
                            a.SyncPos(float.Parse(data[3]), float.Parse(data[4]));
                        }
                    }
                    break;
                case "StartGame":
                    FindObjectOfType<StartPanel>().StartGame();
                    Debug.Log("Game Started");
                    break;
                case "ChangeRow":
                    ChangePlayerRow(data[2], int.Parse(data[3]));
                    break;
                case "SpawnCoin":
                    FindObjectOfType<GameManager>().SpawnCoin(float.Parse(data[2]), float.Parse(data[3]));
                    break;
                case "SpawnBooster":
                    FindObjectOfType<GameManager>().SpawnBooster(int.Parse(data[2]), float.Parse(data[3]), float.Parse(data[4]));
                    break;
                case "PlayerDead":
                    foreach (PlayerManager a in playerListInRoom)
                    {
                        // Refresh player position
                        if (a.playerName == data[2])
                        {
                            // Do something to dead player
                            a.DeadMethod();
                        }
                    }
                    break;
                default:
                    Debug.Log("Unreconized massage : " + massage);
                    break;
            }
        }
    }

    // Sending Massage -----------------------------------------------------------------------------------------
    public void SendMassageClient(string target, string massage)
    {
        string[] data = new string[1];
        data[0] = massage;
        SendMassageClient(target, data);
    }
    public void SendMassageClient(string target, string[] massage)
    {
        // send format : ToWho|Data1|Data2|...
        string data = target;
        // Add data
        for (int i = 0; i < massage.Length; i++)
        {
            data += "|" + massage[i];
        }

        BinaryFormatter formatter = new BinaryFormatter();
        formatter.Serialize(networkStream, data);
        //formatter.Serialize(networkStream, aesEncryption.Encrypt(data));
    }

    // General Method ------------------------------------------------------------------------------------------
    private void SpawnPlayer(string name, int row, int skin, bool needFeedBack)
    {
        GameManager manager = FindObjectOfType<GameManager>();
        PlayerManager tempPlay = Instantiate(manager.playerPrefab[skin]).GetComponent<PlayerManager>();
        tempPlay.playerName = name;
        tempPlay.rowPos = row;
        playerListInRoom.Add(tempPlay);
        Debug.Log(name);
        // Check is it's mine
        if(name == MyName)
        {
            myPlayer = tempPlay;
            return;
        }
        else if (name != MyName && needFeedBack)
        {
            // Send Feedback
            string[] mass = new string[] { "SpawnMyPlayer", myPlayer.playerName, myPlayer.rowPos.ToString(), GameDataLoader.TheData.selectedChar.ToString(), BoolToString(false) };
            SendMassageClient(name, mass);
        }
    }
    public void StartSyncPlayer()
    {
        foreach(PlayerManager a in playerListInRoom)
        {
            if(a.playerName == myPlayer.playerName)
            {
                a.BeginSyncPos();
            }
        }
    }
    public int PlayerCountInRoom()
    {
        return playerListInRoom.Count;
    }
    public void ChangePlayerRow(string thePlayerName, int row)
    {
        foreach(PlayerManager a in playerListInRoom)
        {
            if(a.playerName == thePlayerName)
            {
                a.SetBoolRowChange(row);
            }
        }
    }

    // Custom method to convert sting to bool ----------------------------------------------------------------------------
    // 0 = false ; 1 = true
    private bool StringToBool(string a)
    {
        if (a == "0")
        {
            return false;
        }
        else
        {
            return true;
        }
    }
    private string BoolToString(bool a)
    {
        if (a == false)
        {
            return "0";
        }
        else
        {
            return "1";
        }
    }
}
