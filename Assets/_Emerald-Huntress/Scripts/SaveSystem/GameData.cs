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
    [HideInInspector] public SettingsData settingsData;
    private string settingsFile = "settings.json";
    //private Language _language;

    private XboxJsonCloudSync _saveCloud;

    private void Start()
    {
        _saveCloud = FindAnyObjectByType<XboxJsonCloudSync>();
        _levelSystem = GetComponent<LevelSystem>();
        _levelSystem.gameData = this;
        //_language = GetComponent<Language>();

        //LoadSettings();
        //_language.SetLanguage(settingsData.languageIndex);

        _levelSystem.UpdateLevels();
        LoadGame();
    }

    private void Update()
    {
        //if (Input.GetKeyUp(KeyCode.L)) LoadGame();

        //if (Input.GetKeyUp(KeyCode.S)) SaveGame();
    }

    //Player Game
    public void SaveGame()
    {
        string json = JsonUtility.ToJson(playerGame, true);

        XboxJsonCloudSync.Instance.SaveJson(gameFile, json, (hr) =>
        {
            if (IsSuccess(hr)) Debug.Log("[Client] SAVE ok.");
            else Debug.LogError($"[Client] SAVE falhou: hr=0x{hr:X}");
        });
    }

    public void LoadGame()
    {
        XboxJsonCloudSync.Instance.LoadJson(gameFile, (json, found, hr) =>
        {
            if (!found)
            {
                Debug.LogWarning($"[Client] LOAD sem dados para {gameFile} (hr=0x{hr:X}). Mantendo estado atual.");
                return;
            }

            try
            {
                if(!String.IsNullOrEmpty(json))  playerGame = JsonUtility.FromJson<PlayerGame>(json);
                Debug.Log($"[Client] LOAD ok.");
                _levelSystem.UpdateLevels();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Client] Erro ao aplicar JSON: {ex}");
            }

            //_levelSystem.UpdateLevels();
        });
    }

    /*public void LoadReady()
    {
        string path = GameCheckPath();
        if (File.Exists(path + "/" + gameFile))
        {
            string json = File.ReadAllText(path + "/" + gameFile);
            playerGame = JsonUtility.FromJson<PlayerGame>(json);
        }
        else
        {
            SaveSettings();
        }

        _levelSystem.UpdateLevels();
    }*/

    /*
    //Settings Data
    public void SaveSettings()
    {
        //settingsData.languageIndex = _language.GetLanguageIndex();

        string json = JsonUtility.ToJson(settingsData, true);
        File.WriteAllText(GameCheckPath() + "/" + settingsFile, json);

        //_saveCloud.SaveJsonToCloud(settingsFile, null);
    }

    public void LoadSettings()
    {
        //_saveCloud.LoadJsonFromCloud(settingsFile, LoadSettingsReady, LoadSettingsReady);
    }

    public void LoadSettingsReady()
    {
        string path = GameCheckPath();
        if (File.Exists(path + "/" + settingsFile))
        {
            string json = File.ReadAllText(path + "/" + settingsFile);
            settingsData = JsonUtility.FromJson<SettingsData>(json);
        }
        else
        {
            SaveSettings();
        }
    }*/

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

    // --- Utilitário local para HRESULT ---
    private static bool IsSuccess(int hr) => hr >= 0;
}
