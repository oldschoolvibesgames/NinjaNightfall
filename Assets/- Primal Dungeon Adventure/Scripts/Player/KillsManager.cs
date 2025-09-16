using UnityEngine;

public class KillsManager : MonoBehaviour
{
    public PlayerWeapon playerWeapon;
    public SpecialAreaAttack special;

    public void AddKillByWeapon(int index)
    {
        playerWeapon.RegisterKill(index);
    }
    
    public void AddKill()
    {
        special.RegisterKill();
    }
}
