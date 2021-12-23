using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace Wkwk_Server
{
    class Player
    {
        // Name
        public string playerName;
        // Socket
        public TcpClient tcp;

        // List of online player (not in lobby or room)
        private List<Player> onlineList;
        // List of room of the Games
        private List<Room> roomList;

        // Player position in server List
        // 1 = onlineList
        // 2 = roomList
        public int listPosition;

        // Stream
        public NetworkStream stream;
        // This player room
        private Room myRoom;
        // Is master of room
        private bool isMaster;
        // Is Online
        private bool isOnline;

        // Encryption
        RsaEncryption rsaEncryption;
        public AesEncryption aesEncryption;

        // Private key
        private string ServerPrivateKey;

        // Constructor needed
        public Player(TcpClient tcp, List<Player> onlineList, List<Room> roomList)
        {
            this.tcp = tcp;
            this.onlineList = onlineList;
            this.roomList = roomList;

            stream = tcp.GetStream();
            isOnline = true;

            aesEncryption = new AesEncryption();

            // Load private key
            //ServerPrivateKey = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Private-Key.txt"));
            rsaEncryption = new RsaEncryption(GetPrivateKey());

            try
            {
                // Preparation
                PrepareEncryption();
                // Ask for name and start listening
                AskPlayerName();
            }
            catch
            {
                // Interference from outside
                Server.CloseConnection(this);
            }
        }

        // Preparation Method -------------------------------------------------------------------------
        private void PrepareEncryption()
        {
            // Wait to receive client public key
            BinaryFormatter formatter = new BinaryFormatter();
            string answer = formatter.Deserialize(stream) as string;

            // Decrypt it
            string key = rsaEncryption.Decrypt(answer, rsaEncryption.serverPrivateKey);

            // Save client public key
            rsaEncryption.SetClientPublicKey(key);

            // Create new symmetric key
            aesEncryption.GenerateNewKey();

            // Send it to client
            string encryptKey = rsaEncryption.Encrypt(aesEncryption.ConvertKeyToString(aesEncryption.aesKey), rsaEncryption.clientPublicKey);
            formatter.Serialize(stream, encryptKey);
        }
        private void AskPlayerName()
        {
            // Make the massage
            string massage = aesEncryption.Encrypt("Server|WHORU");
            
            // Send it
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(stream, massage);

            // Waiting for answer
            string answer = aesEncryption.Decrypt(formatter.Deserialize(stream) as string);
            string[] info = answer.Split("|");

            // Add this player to list
            playerName = info[1];
            onlineList.Add(this);
            listPosition = 1;

            // Start listenign player
            StartReceiving();

            // Print massage in console
            Console.WriteLine("Server : Client " + playerName + " is online");
        }
        private String GetPrivateKey()
        {
            string part1 = $"<?xml version=\"1.0\" encoding=\"utf-16\"?>\n  <RSAParameters xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\">\n";
            string part2 = "  <D>jDhuqjJ8bNg3SfWOAtKkEse76Wnw0mDaBmR3gY0nNr9J9Htm0w1SQhsHfi8bZg5A6qRmowAjG/yZ3Ic00zQ8Kf69jmSMyR5pYPlNjYcx8pNH9Qi5LZEIBW/TzK9X9DUyNyd0e1+FcLd9JIAFzu+1pRCGLNOOYVAwHjM/Mo4sPkrACA66V3Y20THLERQ718wIfMKYY+tH3x2VAdPhQGCjcyVKZEqiWOJvYk2JMXShJiz6A6TkXcQM5uuNmTedt+xpMuXb2nlrVA5al0KNl3sB9zW0FkRn52vxo2Y5ULnTE1BoeDuvwiVRX8jAVw56zxO5J09w2EMg2IWmt2M8g/MznQ==</D>\n";
            string part3 = "  <DP>OvOUPTcxVhLdf5kPhJNMuXum1OjC/zBsqehTB5OiH33glI8An36D6/bc1Y+h1c4tTtSO9yANCuxa6BNW3SHn2JdJoNFO9NPi+AI1abfcsPntD8EILvyrCeo5/SnNFJoDYvnfF6aVtyeOaMahvd/SUBcLSQTiOgYeUgHWU+5iuis=</DP>\n";
            string part4 = "  <DQ>F7ZsUIlD84fFUS9Vvp3iBVh01n0yLl67MgA28Dtw8AcA1vHS4k7QkrBQPmKZC6QQPWLZCU7wM7XQw+oOaDc3cULJk8idVTjPvfG6UBv11L2P4lVl5jM3NHgjqYVx/jeXqGKZ9xviFOp1uuY0kDVgcH1+Jyb2KloQG2+FVTCHk18=</DQ>\n";
            string part5 = "  <Exponent>AQAB</Exponent>\n";
            string part6 = "  <InverseQ>rDNIFk9H4AGbvKqAGEzOcTDlDwVMHmCAE64V/U63AeEETQ8l4uYwmPqCbMzk2UTQwQYirmmdQd/mG/aErJ+MkkhEZfGHfb+jlvWTEX3nZ1NWlSB2w6fJHHFI/zaTfqaG+9Na5XWzYY+snHn6kPk6k1iCVv5OKQoizzv+PyFju9w=</InverseQ>\n";
            string part7 = "  <Modulus>2xkVs60nk8a5sdGskTHN2ZVQjiAt4EJ7ZXlbDvz4oeezN1+SII6cVQSIT63U8+5kI8yRPFmpUYwhPAW+3aAV3T1noFvEfRfGuIacOxYu36cZb2+nK85meAGq7qeYKgYOqa0GyTut2RoROVylsSn6OxVHQTColaZMluALXRGZ8JVnUsH8Rq//XkePPvUfvKW2y3ek6VS37SIkEbfjSU/X3kcFu6woTMbEGcRRTsWUOEhhYtIK9DT2tTR4wxVM8OTHWVycixZbJ9F/3Ve9pMtJCvQ3IYH3EUw5VTZMpqqlUF4wHTNYx/hCS1pU4+fnjzP8iyCgrPy8Vh8CK2ETCAhUqQ==</Modulus>\n";
            string part8 = "  <P>/qFRL1mvNEGXZqRrmpzUvIBM8jVqWAdajtsMkWorwFvJCcMLVFzaou4UFb01y6m8KFJu+qsv2UkJTcR9vRksOucVaZWZ0zO1oEmnWlZ1nM14AXPcB3AmLKo0MSDAuDpPSB2YQy3dTn5Uq3AXBaUQCznYA6WwG/Irs2dHlZU+nyM=</P>\n";
            string part9 = "  <Q>3EbU9oOXhjaIZg+QdD9zndOXmi6LGyOUw6JqVcttYkwpAGxYQMvjdf++WNwFBCP94BuFNncAWaC6uDgcPZm7lNGMMd1HOV+41Wq6NypFejoCGc9P7JdK2OVVlk1FsZtDvuloBbpNB1AuD3SkGg0ovqVYBIo541eTVy66dwOev8M=</Q>\n";
            string part10 = "</RSAParameters>";

            return (part1 + part2 + part3 + part4 + part5 + part6 + part7 + part8 + part9 + part10);
        }

        // Check player connection ---------------------------------------------------------------------
        private void CheckConnection()
        {
            while (isOnline)
            {
                Thread.Sleep(5000);
                SendMassage("Server", playerName, "SYNA");
            }
        }

        // Start receiving massage from this player ----------------------------------------------------
        public void StartReceiving()
        {
            Thread recieveThread = new Thread(ReceivedMassage);
            Thread checkConnectionThread = new Thread(CheckConnection);
            recieveThread.Start();
            checkConnectionThread.Start();
        }
        private void ReceivedMassage()
        {
            while (isOnline)
            {
                if (stream.DataAvailable)
                {
                    // Format received : ToWho|Data1|Data2|... 
                    BinaryFormatter formatter = new BinaryFormatter();
                    //string data = aesEncryption.Decrypt(formatter.Deserialize(stream) as string);
                    string data = formatter.Deserialize(stream) as string;
                    string[] info = data.Split("|");

                    // Send data to all
                    if (info[0] == "All")
                    {
                        switch (info[1])
                        {
                            case "SpawnObstacle":
                                string[] a = new string[] { "SpawnObstacle", info[2], info[3], info[4], info[5], info[6] };
                                SendMassage("Client", "All", a);
                                break;
                            case "SpawnObstacleGap":
                                string[] e = new string[] { "SpawnObstacleGap", info[2] };
                                SendMassage("Client", "All", e);                                
                                break;
                            case "StartGame":
                                // Lock room
                                myRoom.SetCanJoin(false);
                                SendMassage("Client", "All", "StartGame");
                                Console.WriteLine(playerName + " : Start Game on Room " + myRoom.roomName);
                                break;
                            case "ChangeRow":
                                string[] b = new string[] { "ChangeRow", playerName, info[2] };
                                SendMassage("Client", "All", b);
                                break;
                            case "SpawnCoin":
                                string[] c = new string[] { "SpawnCoin", info[2], info[3] };
                                SendMassage("Client", "All", c);
                                break;
                            case "SpawnBooster":
                                string[] d = new string[] { "SpawnBooster", info[2], info[3], info[4] };
                                SendMassage("Client", "All", d);
                                break;
                            case "SpawnMovingObs":
                                string[] f = new string[] { "SpawnMovingObs", info[2], info[3], info[4] };
                                SendMassage("Client", "All", f);
                                break;
                            default:
                                Console.WriteLine("Massage not " + info[0] + " correct : " + info[1]);
                                break;
                        }
                    }
                    // Send data to all excep sender
                    else if (info[0] == "AllES")
                    {
                        switch (info[1])
                        {
                            case "SyncPlr":
                                string[] a = new string[] { "SyncPlr", playerName, info[2], info[3] };
                                SendMassage("Client", "AllES", a);
                                break;
                            case "PlayerDead":
                                string[] b = new string[] { "PlayerDead", playerName, info[2], info[3] };
                                SendMassage("Client", "AllES", b);
                                break;
                            default:
                                Console.WriteLine("Massage not " + info[0] + " correct : " + info[1]);
                                break;
                        }
                    }
                    // Send data to server (this)
                    else if (info[0] == "Server")
                    {
                        switch (info[1])
                        {
                            case "Play":
                                // Start matchmaking (actually just finding room)
                                MatchMaking();
                                break;
                            case "CreateRoom":
                                if (info.Length > 4)
                                {
                                    CreateRoom(info[2], info[3], info[4]);
                                }
                                else
                                {
                                    CreateRoom();
                                }
                                break;
                            case "JoinRoom":
                                JoinRoom(info[2]);
                                break;
                            case "SpawnPlayer":
                                SpawnPlayer(int.Parse(info[2]));
                                break;
                            case "ExitRoom":
                                ExitRoom();
                                break;
                            case "ChangeName":
                                Console.WriteLine("Server : " + playerName + " change name to " + info[2]);
                                playerName = info[2];
                                break;
                            default:
                                Console.WriteLine("Massage not " + info[0] + " correct : " + info[1]);
                                break;
                        }
                    }
                    // Send data to specific player
                    else
                    {
                        for(int i = 0; i < myRoom.playerList.Count; i++)
                        {
                            if (myRoom.playerList[i].playerName == info[0])
                            {
                                switch (info[1])
                                {
                                    case "SpawnMyPlayer":
                                        string[] theData = new string[] { "SpawnPlayer", info[2], info[3], info[4], BoolToString(false) };
                                        SendMassage("Client", myRoom.playerList[i].playerName, theData);
                                        // Print massage
                                        //Console.WriteLine(playerName + " : Send Object Player to " + myRoom.playerList[i].playerName);
                                        break;
                                    default:

                                        break;
                                }
                            }
                        }
                    }
                }
            }
        }

        // Send massage ---------------------------------------------------------------------------------
        private void SendMassage(string fromWho, string target, string massage)
        {
            string[] data = new string[1];
            data[0] = massage;
            SendMassage(fromWho, target, data);
        }
        private void SendMassage(string fromWho, string target, string[] massage)
        {
            if (isOnline)
            {
                // Send to this player
                if (target == playerName)
                {
                    // send format : FromWho|Data1|Data2|...
                    string data = fromWho;
                    // Add data
                    for (int i = 0; i < massage.Length; i++)
                    {
                        data += "|" + massage[i];
                    }

                    SendSerializationDataHandler(stream, data);
                }

                // Send to All Player in room
                else if (target == "All")
                {
                    try
                    {
                        // send format : FromWho|Data1|Data2|...
                        string data = fromWho;
                        // Add data
                        for (int i = 0; i < massage.Length; i++)
                        {
                            data += "|" + massage[i];
                        }

                        // Send to all
                        for (int i = 0; i < myRoom.playerList.Count; i++)
                        {
                            SendSerializationDataHandler(myRoom.playerList[i].stream, data);
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Send All error : " + e.Message);
                    }
                }

                // Send to All Player in room excep sender (this)
                else if (target == "AllES")
                {
                    try
                    {
                        if(myRoom != null)
                        {
                            for (int i = 0; i < myRoom.playerList.Count; i++)
                            {
                                if (myRoom.playerList[i].playerName != playerName)
                                {
                                    // send format : FromWho|Data1|Data2|...
                                    string data = fromWho;
                                    // Add data
                                    for (int j = 0; j < massage.Length; j++)
                                    {
                                        data += "|" + massage[j];
                                    }
                                    // Send massage
                                    SendSerializationDataHandler(myRoom.playerList[i].stream, data);
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Send AllES error : " + e.Message);
                    }
                }

                // Send to specific client
                else
                {
                    try
                    {
                        for (int i = 0; i < myRoom.playerList.Count; i++)
                        {
                            if (myRoom.playerList[i].playerName == target)
                            {
                                // send format : FromWho|Data1|Data2|...
                                string data = fromWho;
                                // Add data
                                for (int j = 0; j < massage.Length; j++)
                                {
                                    data += "|" + massage[j];
                                }

                                // Send massage
                                SendSerializationDataHandler(myRoom.playerList[i].stream, data);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Send to " + target + " error : " + e.Message);
                    }
                }
            }
        }
        private void SendSerializationDataHandler(NetworkStream Thestream, string Thedata)
        {
            try
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(Thestream, Thedata);
            }
            catch(Exception e)
            {
                Console.WriteLine("Send massage error from " + playerName + " : " + e.Message);
                // Disconnect client from server
                DisconnectFromServer();
            }
        }

        // Room ------------------------------------------------------------------------------------------
        // Matchmaking
        public void MatchMaking()
        {
            // Check if there is room in list
            if(roomList.Count > 0)
            {
                // Check if there is room can join
                for(int i = 0; i < roomList.Count; i++)
                {
                    if (roomList[i].canJoin && roomList[i].isPublic)
                    {
                        onlineList.Remove(this);
                        roomList[i].playerList.Add(this);
                        listPosition = 2;
                        roomList[i].CheckRoom();
                        myRoom = roomList[i];

                        // Send massage to client
                        SendMassage("Server", playerName, "JoinedRoom|" + myRoom.roomName);

                        // Print massage
                        Console.WriteLine(playerName + " : Joined room " + myRoom.roomName);

                        return;
                    }
                }
            }

            // Just make new room if there is no room can be joined
            CreateRoom();
        }        
        // Create room auto
        public void CreateRoom()
        {
            CreateRoom(playerName + "Room", "5", "1");
        }
        // Create custom private room
        public void CreateRoom(string roomName, string playerMax, string isPublic)
        {
            // Generate room
            Room temp = new Room();
            temp.roomName = roomName;
            temp.MaxPlayer = int.Parse(playerMax);
            temp.isPublic = StringToBool(isPublic);
            temp.playerList = new List<Player>();
            temp.SetCanJoin(true);

            // Moving player from online to new room
            foreach (Player a in onlineList)
            {
                if (a.tcp == tcp)
                {
                    // Remove from online list
                    onlineList.Remove(a);

                    // Add to room list
                    temp.playerList.Add(a);
                    listPosition = 2;

                    // Save room
                    myRoom = temp;
                    isMaster = true;
                    roomList.Add(myRoom);

                    // Send massage to client
                    SendMassage("Server", playerName, "CreatedRoom");

                    // Print massage in server
                    Console.WriteLine(playerName + " : Created room " + myRoom.roomName);

                    return;
                }
            }
        }
        // Join other players room
        public void JoinRoom(string roomName)
        {
            foreach (Room a in roomList)
            {
                if (a.roomName == roomName && a.canJoin)
                {
                    onlineList.Remove(this);
                    a.playerList.Add(this);
                    listPosition = 2;
                    a.CheckRoom();
                    myRoom = a;

                    // Send massage to client
                    SendMassage("Server", playerName, "JoinedRoom|" + myRoom.roomName);

                    // Print massage
                    Console.WriteLine(playerName + " : Joined room " + myRoom.roomName);

                    return;
                }
            }

            // If not join room
            SendMassage("Server", playerName, "RoomNotFound");
        }
        // Exit from room
        public void ExitRoom()
        {
            foreach (Player a in myRoom.playerList)
            {
                if (a.tcp == tcp)
                {
                    // If this is master, set other player to master
                    if (isMaster)
                    {
                        for (int i = 0; i < myRoom.playerList.Count; i++)
                        {
                            if (myRoom.playerList[i].playerName != playerName)
                            {
                                myRoom.playerList[i].SetToMaster();
                            }
                        }
                    }

                    // Remove from player list in room
                    myRoom.playerList.Remove(a);

                    // Print massage
                    Console.WriteLine(playerName + " : Exit from room " + myRoom.roomName);

                    // Destroy room if needed
                    if (myRoom.playerList.Count <= 0)
                    {
                        Server.DestroyRoom(myRoom, roomList);
                    }

                    // Add to online list
                    onlineList.Add(a);
                    listPosition = 1;

                    myRoom = null;

                    // Send Massage To Client
                    SendMassage("Server", playerName, "ExitRoom");

                    return;
                }
            }
        }

        // Others Method -----------------------------------------------------------------------------------------------
        // Spawning player in random start position (0-4)
        private void SpawnPlayer(int selectedChar)
        {
            // Random pos
            Random rand = new Random();
            int randPos = rand.Next(5);

            // If it's used
            while(myRoom.randomPosUsed[randPos])
            {
                randPos = rand.Next(5);
            }

            // If it's not
            myRoom.randomPosUsed[randPos] = true;
            string[] massage = new string[] { "SpawnPlayer", playerName, randPos.ToString(), selectedChar.ToString(), BoolToString(true) };
            // Send massage to all player
            SendMassage("Server", "All", massage);

            // Print massage in server
            Console.WriteLine(playerName + " : Request Spawn Player, Get-" + randPos);
        }
        // Disconnect from server
        private void DisconnectFromServer()
        {
            // Send Massage to other
            string[] m = { "Disconnect", playerName };
            SendMassage("Server", "AllES", m);
            // Print massage
            Console.WriteLine(playerName + " : Disconnected from server");
            isOnline = false;
            if (listPosition == 1)
            {
                Server.DisconnectFromServer(this, onlineList);
            }
            else if (listPosition == 2)
            {
                // If this is master, set other player to master
                if (isMaster)
                {
                    for(int i = 0; i < myRoom.playerList.Count; i++)
                    {
                        if(myRoom.playerList[i].playerName != playerName)
                        {
                            myRoom.playerList[i].SetToMaster();
                        }
                    }
                }

                Server.DisconnectFromServer(this, myRoom, roomList);
            }
        }
        // Set this player to master
        public void SetToMaster()
        {
            isMaster = true;
            SendMassage("Server", playerName, "SetToMaster");
            // print massage
            Console.WriteLine(playerName + " : Become a master of room " + myRoom.roomName);
        }

        // Custom method to convert sting to bool -----------------------------------------------------------------
        // 0 = false ; 1,2,3...n = true
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
}
