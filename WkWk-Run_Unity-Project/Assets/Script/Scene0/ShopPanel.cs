using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopPanel : MonoBehaviour
{
    private MainMenuManager manager;
    private Client network;

    [SerializeField] private Text coinText;
    [SerializeField] private Text selectedText;
    [SerializeField] private Image selectedImage;
    [SerializeField] private Text descText;

    [SerializeField] private GameObject selectButton;
    [SerializeField] private GameObject buyButton;
    [SerializeField] private Text buyButtonText;

    private SaveData saveData;

    // Scroll control
    [SerializeField] private GameObject scrollBar;
    [SerializeField] private GameObject content;
    float dist;
    float scrollPos = 0;
    float[] allScrollPos;
    int SelectedPos;

    private void Start()
    {
        manager = FindObjectOfType<MainMenuManager>();
        network = FindObjectOfType<Client>();
        saveData = GameDataLoader.TheData;

        coinText.text = saveData.Coin.ToString("n0");
        selectedText.text = saveData.characterName[saveData.selectedChar];
        selectedImage.sprite = manager.characterSprite[saveData.selectedChar];
        descText.text = saveData.characterDesc[saveData.selectedChar];

        allScrollPos = new float[saveData.characterName.Length];
        dist = 1f / (allScrollPos.Length - 1f);
        for (int i = 0; i < allScrollPos.Length; i++)
        {
            allScrollPos[i] = dist * i;
        }
        SelectedPos = saveData.selectedChar;
        scrollBar.GetComponent<Scrollbar>().value = allScrollPos[saveData.selectedChar];
        scrollPos = allScrollPos[saveData.selectedChar];

        buyButton.SetActive(false);
    }

    private void Update()
    {
        // Scroll controll
        if (Input.GetMouseButton(0))
        {
            scrollPos = scrollBar.GetComponent<Scrollbar>().value;
        }
        else
        {
            for (int i = 0; i < allScrollPos.Length; i++)
            {
                if (scrollPos < allScrollPos[i] + (dist / 2) && scrollPos > allScrollPos[i] - (dist / 2))
                {
                    scrollBar.GetComponent<Scrollbar>().value = Mathf.Lerp(scrollBar.GetComponent<Scrollbar>().value, allScrollPos[i], .08f);
                    ChangeImageAndText(i);
                }
            }
        }

        for (int i = 0; i < allScrollPos.Length; i++)
        {
            if (scrollPos < allScrollPos[i] + (dist / 2) && scrollPos > allScrollPos[i] - (dist / 2))
            {
                content.transform.GetChild(i).localScale = Vector2.Lerp(content.transform.GetChild(i).localScale, new Vector2(1, 1), .03f);
            }
            else
            {
                content.transform.GetChild(i).localScale = Vector2.Lerp(content.transform.GetChild(i).localScale, new Vector2(.65f, .65f), .03f);
            }
        }
    }

    private void ChangeImageAndText(int value)
    {
        selectedText.text = saveData.characterName[value];
        selectedImage.sprite = manager.characterSprite[value];
        descText.text = saveData.characterDesc[value];
        SelectedPos = value;

        if (saveData.SkinIsLock[value])
        {
            buyButton.SetActive(true);
            selectButton.SetActive(false);
            buyButtonText.text = saveData.SkinPrice[value].ToString();
        }
        else
        {
            buyButton.SetActive(false);
            selectButton.SetActive(true);
            if (saveData.selectedChar == value)
            {
                selectButton.GetComponentInChildren<Text>().text = "Selected";
            }
            else
            {
                selectButton.GetComponentInChildren<Text>().text = "Select";
            }
        }
    }

    public void SelectCharacter()
    {
        saveData.selectedChar = SelectedPos;
        SaveGame.SaveProgress(saveData);
        selectButton.GetComponentInChildren<Text>().text = "Selected";
        manager.ChangeSelectedCharacter(SelectedPos);
    }
    public void BuyCharacter()
    {
        if (saveData.Coin > saveData.SkinPrice[SelectedPos])
        {
            saveData.Coin -= saveData.SkinPrice[SelectedPos];
            saveData.SkinIsLock[SelectedPos] = false;
            SaveGame.SaveProgress(saveData);

            buyButton.SetActive(false);
            selectButton.SetActive(true);
            selectButton.GetComponentInChildren<Text>().text = "Selected";

            coinText.text = saveData.Coin.ToString("n0");
            manager.ChangeCoinValue(saveData.Coin);
        }
    }

    public void NextPrevCharacter(bool isNext)
    {
        if (isNext && SelectedPos <= 3)
        {
            SelectedPos += 1;
        }
        else if (!isNext && SelectedPos >= 1)
        {
            SelectedPos -= 1;
        }
        
        scrollBar.GetComponent<Scrollbar>().value = allScrollPos[SelectedPos];
        scrollPos = allScrollPos[SelectedPos];
        ChangeImageAndText(SelectedPos);
    }
    
}
