using UnityEngine;
using UnityEngine.Events;

public class PlayerLife : MonoBehaviour
{
    public LifeUI[] lifes;
    public int playerLife = 1;
    public int playerMaxLifes = 3;
    public UnityEvent onDamage;

    private void Awake()
    {
        UpdateLife();
    }

    public void Damage(int damage)
    {
        playerLife -= damage;
        onDamage?.Invoke();
        UpdateLife();
    }

    public void Healing(int healing)
    {
        playerLife += healing;
        UpdateLife();
    }

    public void AddLifeSlot()
    {
        playerMaxLifes++;
        playerLife = playerMaxLifes;
        UpdateLife();
    }

    public void UpdateLife()
    {
        foreach (var life in lifes)
        {
            life.gameObject.SetActive(false);
        }

        for (int i = 0; i < playerMaxLifes; i++)
        {
            lifes[i].gameObject.SetActive(true);
        }
        
        if(playerLife < 0) Death();
        else if (playerLife >= playerMaxLifes) playerLife = playerMaxLifes;

        foreach (var life in lifes)
        {
            life.SetLife(false);
        }

        for (int i = 0; i < playerLife; i++)
        {
            lifes[i].SetLife(true);
        }
    }

    public void RestoreAllLife()
    {
        playerLife = playerMaxLifes;
        UpdateLife();
    }

    private void Death()
    {
        
    }
}
