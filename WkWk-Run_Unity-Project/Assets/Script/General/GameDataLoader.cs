using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameDataLoader : MonoBehaviour
{
    public static SaveData TheData { get; set; }

    private void Awake()
    {
        TheData = SaveGame.LoadData();

        DontDestroyOnLoad(gameObject);
    }

    public static void ReloadGameData()
    {
        TheData = SaveGame.LoadData();
    }
}
