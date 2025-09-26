using UnityEngine;

public class KillsManager : MonoBehaviour
{
    public SpecialAreaAttack special;

    public void AddKillByWeapon(int index)
    {
        
    }
    
    public void AddKill()
    {
        special.RegisterKill();
    }
}
