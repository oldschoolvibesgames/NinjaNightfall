using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class GameData : MonoBehaviour
{
    //Player
    public PlayerGame playerGame;
    private string gameFile = "game.json";
    private LevelSystem _levelSystem;
    
    //Settigs
    [HideInInspector]public SettingsData settingsData;
    private string settingsFile = "settings.json";
    //private Language _language;

    private void Awake()
    {
        _levelSystem = GetComponent<LevelSystem>();
        _levelSystem.gameData = this;
        //_language = GetComponent<Language>();
        
        LoadSettings();
        //_language.SetLanguage(settingsData.languageIndex);
        
        LoadGame();
        _levelSystem.UpdateLevels();
    }
    
    //Player Game
    public string GameCheckPath()
    {
        string path = Application.persistentDataPath;
        if (!Directory.Exists(path)) Directory.CreateDirectory(path);
        Debug.Log("Game path: " + path);
        
        return path;
    }
        
    public void SaveGame()
    {
        string json = JsonUtility.ToJson(playerGame, true);
        File.WriteAllText(SettingsCheckPath() + "/" + gameFile, json);
    }

    public void LoadGame()
    {
        string path = SettingsCheckPath();
        if (File.Exists(path + "/" + gameFile))
        {
            string json = File.ReadAllText(path + "/" + gameFile);
            playerGame = JsonUtility.FromJson<PlayerGame>(json);
        }
        else
        {
            SaveSettings();
        }
    }

    //Settings Data
    public string SettingsCheckPath()
    {
        string path = Application.persistentDataPath + "/Settings";
        if (!Directory.Exists(path)) Directory.CreateDirectory(path);
        Debug.Log("Settings path: " + path);
        
        return path;
    }
        
    public void SaveSettings()
    {
        //settingsData.languageIndex = _language.GetLanguageIndex();
        
        string json = JsonUtility.ToJson(settingsData, true);
        File.WriteAllText(SettingsCheckPath() + "/" + settingsFile, json);
    }

    public void LoadSettings()
    {
        string path = SettingsCheckPath();
        if (File.Exists(path + "/" + settingsFile))
        {
            string json = File.ReadAllText(path + "/" + settingsFile);
            settingsData = JsonUtility.FromJson<SettingsData>(json);
        }
        else
        {
            SaveSettings();
        }
    }
    
    [Serializable]
    public class SettingsData
    {
        public int languageIndex = 0;
        public float volumeMusic = 1f;
        public float volumeSFX = 1f;
    }

    [Serializable]
    public class PlayerGame
    {
        public string currentLevel;
        
        public List<LevelStatus> levels = new List<LevelStatus>();
        public float playerGameTime = 0f;
    }

    [Serializable]
    public class LevelStatus
    {
        public string levelName;
        public bool isLocked = false;
        public bool isDone = false;

        public void ConstructLevel(string newName, bool locked, bool done)
        {
            levelName = newName;
            isLocked = locked;
            isDone = done;
        }
    }
}
