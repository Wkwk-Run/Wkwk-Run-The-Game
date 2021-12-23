using UnityEngine;

// Contain user data

[System.Serializable]
public class SaveData
{
    // User name
    public string UserName { get; set; }
    // User coin
    public int Coin { get; set; }

    // User skin
    public bool[] SkinIsLock = new bool[5];
    public int[] SkinPrice = new int[] { 0, 1000, 2000, 2500, 3000 };
    public int selectedChar { get; set; }

    // Character
    public string[] characterName { get; private set; }
    public string[] characterDesc { get; private set; }

    // Constructor
    public SaveData()
    {
        UserName = "";
        Coin = 500;

        int i = 0;
        foreach (bool a in SkinIsLock)
        {
            SkinIsLock[i] = new bool();
            if (i == 0)
            {
                SkinIsLock[i] = false;
            }
            else
            {
                SkinIsLock[i] = true;
            }
            i++;
        }

        selectedChar = 0;

        characterName = new string[] { "Bocil", "Cewe Kepang", "Emak Kos", "Pak Ustadz", "Pocong" };
        characterDesc = new string[] { "Little boy who likes adventure", "Bocil chilhood friend", "Old lady who owned inn",
                                           "Muslim religious leader", "Jumping ghost covered with white shroud"};
    }
}
