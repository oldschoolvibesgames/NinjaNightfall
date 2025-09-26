using MoreMountains.CorgiEngine;
using UnityEngine;
using UnityEngine.Events;

public class PlayerHealth : MonoBehaviour
{
    [Header("Refs")]
    public Health corgiHealth;
    public RespawnObjectManager respawnObjectManager;
    
    [Header("Settings")]
    public int startLife = 3;

    [Header("Events")]
    public UnityEvent onReciveDamage;

    private LifeUI _lifeUI;
    private PlayerData _playerData;

    private void Start()
    {
        _playerData = FindAnyObjectByType<PlayerData>();
        _playerData.LoadAll();
        _lifeUI = FindAnyObjectByType<LifeUI>();
        corgiHealth.InitialHealth = _playerData.playerProgressData.lifeData.maxLife;
        corgiHealth.MaximumHealth = _playerData.playerProgressData.lifeData.maxLife;
        corgiHealth.CurrentHealth = _playerData.playerProgressData.lifeData.maxLife;
        
        corgiHealth.OnRevive += RestoreAllLife;
        corgiHealth.OnHit += DamageCorgi;
        UpdateUI();
    }

    public void Haling(int healing)
    {
        corgiHealth.CurrentHealth += healing;
        if (corgiHealth.CurrentHealth > corgiHealth.MaximumHealth) corgiHealth.CurrentHealth = corgiHealth.MaximumHealth;
        UpdateUI();
    }

    public void AddLifeSlot()
    {
        corgiHealth.MaximumHealth++;
        if (corgiHealth.MaximumHealth > 7.0f) corgiHealth.MaximumHealth = 7;
        corgiHealth.InitialHealth = corgiHealth.MaximumHealth;
        corgiHealth.CurrentHealth = corgiHealth.MaximumHealth;
        UpdateUI();

        _playerData.playerProgressData.lifeData.maxLife = (int)corgiHealth.MaximumHealth;
    }

    public void DamageCorgi()
    {
        UpdateUI();
        onReciveDamage?.Invoke();
    }
    

    public void OnDeath()
    {
        corgiHealth.Kill();
    }

    public void RestoreAllLife()
    {
        respawnObjectManager.RespawnObj();
        UpdateUI();
    }

    private void UpdateUI()
    {
        _lifeUI.UpdateUI((int)corgiHealth.CurrentHealth, (int)corgiHealth.MaximumHealth);
    }
}
