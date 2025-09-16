using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class PlayerData : MonoBehaviour
{
    public PlayerProgressData playerProgressData = new PlayerProgressData();
    private string saveFile = "player_data.json";

    [Header("Player References")]
    public PlayerLife playerLife;
    public PlayerWeapon playerWeapon;
    public SpecialAreaAttack specialAttack;

    public void SaveAll()
    {
        ValidateReferences();

        // Life
        if (playerLife != null)
        {
            playerProgressData.lifeData.currentLife = playerLife.playerLife;
            playerProgressData.lifeData.maxLife = playerLife.playerMaxLifes;
        }

        // Weapons
        if (playerWeapon != null && playerWeapon.weapons != null)
        {
            playerProgressData.weaponData.Clear();
            foreach (var weapon in playerWeapon.weapons)
            {
                var weaponSave = new WeaponSaveData
                {
                    ammo = weapon.ammo,
                    countKills = weapon.countKills,
                    unlocked = weapon.unlocked
                };
                playerProgressData.weaponData.Add(weaponSave);
            }
        }

        // Special Attack
        if (specialAttack != null)
        {
            playerProgressData.specialData.unlocked = specialAttack.unlocked;
            playerProgressData.specialData.isReady = specialAttack.isReady;
            playerProgressData.specialData.infinite = specialAttack.infinite;

            var killCountField = typeof(SpecialAreaAttack).GetField("_killCount", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (killCountField != null)
            {
                playerProgressData.specialData.killCount = (int)killCountField.GetValue(specialAttack);
            }
        }

        string json = JsonUtility.ToJson(playerProgressData, true);
        File.WriteAllText(CheckSavePath() + "/" + saveFile, json);
        Debug.Log("Dados salvos.");
    }

    public void LoadAll()
    {
        ValidateReferences();

        string path = CheckSavePath();
        if (File.Exists(path + "/" + saveFile))
        {
            string json = File.ReadAllText(path + "/" + saveFile);
            playerProgressData = JsonUtility.FromJson<PlayerProgressData>(json);

            // Life
            if (playerLife != null)
            {
                playerLife.playerLife = playerProgressData.lifeData.currentLife;
                playerLife.playerMaxLifes = playerProgressData.lifeData.maxLife;
                playerLife.UpdateLife();
            }

            // Weapons
            if (playerWeapon != null && playerWeapon.weapons != null)
            {
                for (int i = 0; i < playerWeapon.weapons.Length; i++)
                {
                    if (i >= playerProgressData.weaponData.Count) break;

                    var saved = playerProgressData.weaponData[i];
                    var weapon = playerWeapon.weapons[i];

                    weapon.ammo = saved.ammo;
                    weapon.countKills = saved.countKills;
                    weapon.unlocked = saved.unlocked;
                }
            }

            // Special Attack
            if (specialAttack != null)
            {
                specialAttack.unlocked = playerProgressData.specialData.unlocked;
                specialAttack.isReady = playerProgressData.specialData.isReady;
                specialAttack.infinite = playerProgressData.specialData.infinite;

                var killCountField = typeof(SpecialAreaAttack).GetField("_killCount", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (killCountField != null)
                {
                    killCountField.SetValue(specialAttack, playerProgressData.specialData.killCount);
                }
            }

            Debug.Log("Dados carregados.");
        }
        else
        {
            Debug.LogWarning("Nenhum dado salvo encontrado. Usando configurações padrão.");
            SaveAll();
        }
    }

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

    private void ValidateReferences()
    {
        if (playerLife == null)
            playerLife = FindAnyObjectByType<PlayerLife>();

        if (playerWeapon == null)
            playerWeapon = FindAnyObjectByType<PlayerWeapon>();

        if (specialAttack == null)
            specialAttack = FindAnyObjectByType<SpecialAreaAttack>();
    }

    private string CheckSavePath()
    {
        string path = Application.persistentDataPath + "/PlayerData";
        if (!Directory.Exists(path)) Directory.CreateDirectory(path);
        return path;
    }

    [Serializable]
    public class PlayerProgressData
    {
        public LifeData lifeData = new LifeData();
        public List<WeaponSaveData> weaponData = new List<WeaponSaveData>();
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
    public class WeaponSaveData
    {
        public int ammo;
        public int countKills;
        public bool unlocked;
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
}
