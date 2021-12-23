using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Analytics;

public class CreateRoomPanel : MonoBehaviour
{
    [SerializeField] private InputField inputRoomName;
    [SerializeField] private GameObject confirmButton;
    [SerializeField] private Text maxPlayer;
    private int maxPlayerCount;
    [SerializeField] private Text visibility;
    private bool isPublic;
    private Client network;

    private void Start()
    {
        network = FindObjectOfType<Client>();
        confirmButton.SetActive(false);
        maxPlayerCount = int.Parse(maxPlayer.text);
        if(visibility.text == "Public")
        {
            isPublic = true;
        }
        else
        {
            isPublic = false;
        }
    }

    public void MaxMinName()
    {
        if (inputRoomName.text.Length < 4 || inputRoomName.text.Length > 14)
        {
            confirmButton.SetActive(false);
        }
        else
        {
            confirmButton.SetActive(true);
        }
    }

    public void PlusMaxPlayer()
    {
        if (maxPlayerCount < 5)
        {
            maxPlayerCount++;
            maxPlayer.text = maxPlayerCount.ToString();
        }
    }
    public void MinMaxPlayer()
    {
        if (maxPlayerCount > 2)
        {
            maxPlayerCount--;
            maxPlayer.text = maxPlayerCount.ToString();
        }
    }

    public void ChangeVisibility()
    {
        if(visibility.text == "Public")
        {
            visibility.text = "Private";
        }
        else
        {
            visibility.text = "Public";
        }
    }

    // Custom method to convert bool to string
    // 0 = false ; 1 = true
    private string BoolToString(bool a)
    {
        if (a)
        {
            return "1";
        }
        else
        {
            return "0";
        }
    }

    public void CreateRoom()
    {
        // Send massage to server
        StartPanel.RoomName = inputRoomName.text;
        string[] massage = new string[] { "CreateRoom", inputRoomName.text, maxPlayerCount.ToString(), BoolToString(isPublic) };
        network.SendMassageClient("Server", massage);

        // Analytic
        Analytics.CustomEvent("Play Create Room");
    }
}
