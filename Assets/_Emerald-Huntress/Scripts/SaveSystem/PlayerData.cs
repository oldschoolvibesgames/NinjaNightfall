using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using static GameData;

public class PlayerData : MonoBehaviour
{
    public PlayerProgressData playerProgressData = new PlayerProgressData();
    private string saveFile = "player_data.json";
    private XboxJsonCloudSync _saveCloud;

    private void Awake()
    {
        _saveCloud = FindAnyObjectByType<XboxJsonCloudSync>();
    }

    public void SaveAll()
    {
        string json = JsonUtility.ToJson(playerProgressData, true);

        XboxJsonCloudSync.Instance.SaveJson(saveFile, json, (hr) =>
        {
            if (IsSuccess(hr)) Debug.Log("[Client] SAVE ok.");
            else Debug.LogError($"[Client] SAVE falhou: hr=0x{hr:X}");
        });
    }

    public void LoadAll()
    {
        XboxJsonCloudSync.Instance.LoadJson(saveFile, (json, found, hr) =>
        {
            if (!found)
            {
                Debug.LogWarning($"[Client] LOAD sem dados para {saveFile} (hr=0x{hr:X}). Mantendo estado atual.");
                return;
            }

            try
            {
                if (!String.IsNullOrEmpty(json)) playerProgressData = JsonUtility.FromJson<PlayerProgressData>(json);
                Debug.Log($"[Client] LOAD ok.");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Client] Erro ao aplicar JSON: {ex}");
            }
        });
    }
    /*
    public void LoadReady()
    {
        string path = CheckSavePath();
        if (File.Exists(path + "/" + saveFile))
        {
            string json = File.ReadAllText(path + "/" + saveFile);
            playerProgressData = JsonUtility.FromJson<PlayerProgressData>(json);

            Debug.Log("Dados carregados.");
        }
        else
        {
            Debug.LogWarning("Nenhum dado salvo encontrado. Usando configurações padrão.");
            SaveAll();
        }
    }*/

    public bool IsItemCollected(int index)
    {
        if (index >= 0 && index < playerProgressData.specialItems.Count)
        {
            return playerProgressData.specialItems[index].collected;
        }

        Debug.LogWarning($"Índice inválido para item especial: {index}");
        return false;
    }

    public void SetItemCollected(int index, bool state)
    {
        if (index >= 0 && index < playerProgressData.specialItems.Count)
        {
            playerProgressData.specialItems[index].collected = state;
        }
        else
        {
            Debug.LogWarning($"Índice inválido para item especial: {index}");
        }
    }

    private string CheckSavePath()
    {
        string path = Application.persistentDataPath;
        if (!Directory.Exists(path)) Directory.CreateDirectory(path);
        return path;
    }

    [Serializable]
    public class PlayerProgressData
    {
        public LifeData lifeData = new LifeData();
        public SpecialSaveData specialData = new SpecialSaveData();
        public List<SpecialItem> specialItems = new List<SpecialItem>();
    }

    [Serializable]
    public class LifeData
    {
        public int currentLife = 1;
        public int maxLife = 3;
    }

    [Serializable]
    public class SpecialSaveData
    {
        public bool unlocked;
        public bool isReady;
        public bool infinite;
        public int killCount;
    }

    [Serializable]
    public class SpecialItem
    {
        public string itemName;
        public bool collected;
    }

    // --- Utilitário local para HRESULT ---
    private static bool IsSuccess(int hr) => hr >= 0;
}
