using System;
using System.Collections.Generic;
using MoreMountains.CorgiEngine;
using UnityEngine;
using UnityEngine.Events;

public class PlayerHealth : MonoBehaviour
{
    [Header("Refs")]
    public Animator animator;
    public Health corgiHealth;
    public RespawnObjectManager respawnObjectManager;
    
    [Header("Settings")]
    public int startLife = 3;
    public CountdownStandard immortalTimer;

    [Header("Events")]
    public UnityEvent onReciveDamage;

    private PlayerLife _playerLife;
    private bool _immortal;

    private void Awake()
    {
        _playerLife = FindAnyObjectByType<PlayerLife>();
        _playerLife.playerLife = _playerLife.playerMaxLifes;
        _playerLife.UpdateLife();
        
        immortalTimer.onTimeEnd.AddListener(DisableImmortal);

        corgiHealth.OnRevive += RestoreAllLife;
        corgiHealth.OnHit += DamageCorgi;
        corgiHealth.MaximumHealth = _playerLife.playerMaxLifes;
        corgiHealth.CurrentHealth = _playerLife.playerLife;
    }

    private void Update()
    {
        immortalTimer.UpdateClass();
    }

    public void DisableImmortal()
    {
        _immortal = false;
    }

    public void Haling(int healing)
    {
        _playerLife.Healing(healing);
        corgiHealth.CurrentHealth = _playerLife.playerLife;
    }

    public void AddLifeSlot()
    {
        _playerLife.AddLifeSlot();
        corgiHealth.MaximumHealth++;
        corgiHealth.CurrentHealth = corgiHealth.MaximumHealth;
    }

    public void DamageCorgi()
    {
        _playerLife.playerMaxLifes = (int)corgiHealth.MaximumHealth;
        _playerLife.playerLife = (int)corgiHealth.CurrentHealth;
        _playerLife.UpdateLife();
        onReciveDamage?.Invoke();
    }
    

    public void OnDeath()
    {
        corgiHealth.Kill();
    }

    public void RestoreAllLife()
    {
        _playerLife.RestoreAllLife();
        respawnObjectManager.RespawnObj();
    }
}
