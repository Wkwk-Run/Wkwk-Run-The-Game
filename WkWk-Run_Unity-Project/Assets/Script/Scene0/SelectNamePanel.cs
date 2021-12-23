using UnityEngine;
using UnityEngine.UI;

public class SelectNamePanel : MonoBehaviour
{
    [SerializeField] private InputField inputName;
    [SerializeField] private GameObject confirmButton;
    [SerializeField] private GameObject backButton;

    private void Start()
    {
        confirmButton.SetActive(false);

        if (GameDataLoader.TheData.UserName == "")
        {
            backButton.SetActive(false);
        }
        else
        {
            backButton.SetActive(true);
        }
    }

    // Save name
    public void SelectName()
    {
        GameDataLoader.TheData.UserName = inputName.text;
        SaveGame.SaveProgress(GameDataLoader.TheData);
        FindObjectOfType<MainMenuManager>().nameText.text = inputName.text;
        Client temp = FindObjectOfType<Client>();
        temp.MyRealName = inputName.text;
        temp.GenerateName();

        // Tell server
        Client client = FindObjectOfType<Client>();
        string[] massage = new string[] { "ChangeName", temp.MyName };
        client.SendMassageClient("Server", massage);

        gameObject.SetActive(false);
    }

    // Check name character size
    public void MaxMinName()
    {
        if(inputName.text.Length < 3 || inputName.text.Length > 10)
        {
            confirmButton.SetActive(false);
        }
        else
        {
            confirmButton.SetActive(true);
        }
    }
}
