using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class Storage : MonoBehaviour
{
    public static Storage Instance { get; private set; }

    public int money = 0;
    public int moves = 0;

    private string filePath;
    private BinaryFormatter formatter;

    private const string SaveKey = "game_data";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            filePath = Path.Combine(Application.persistentDataPath, "saveData.json");
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private string GetPlayerSaveKey()
    {
        return $"{SaveKey}";
    }
    public void Save()
    {
        string json = JsonUtility.ToJson(this);
        PlayerPrefs.SetString(GetPlayerSaveKey(), json);
        PlayerPrefs.Save();
    }

    public void Load()
    {
        if (PlayerPrefs.HasKey(GetPlayerSaveKey()))
        {
            string json = PlayerPrefs.GetString(GetPlayerSaveKey());
            JsonUtility.FromJsonOverwrite(json, Instance);

        }
        else
        {

            ResetSave();
        }

    }

   
    public void ResetSave()
    {
        money = 0;
        moves = 0;
        Save();
    }
}
