using UnityEngine;
using UnityEngine.Events;

public class Ammo : MonoBehaviour
{
    public int weaponIndex;
    public int amount;
    public UnityEvent onCollected;
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.CompareTag("Player"))
        {
            var weapon = other.GetComponent<PlayerWeapon>();
            
            //Unlock wapon:
            if (!weapon.weapons[weaponIndex].unlocked)
            {
                weapon.weapons[weaponIndex].unlocked = true;
                if (weaponIndex == 1)
                {
                    weapon.weapons[weaponIndex].ammo = 5;
                }
                
                onCollected?.Invoke();
                this.gameObject.SetActive(false);
                
                return;
            }
            
            
            if(weapon.weapons[weaponIndex].ammo >= weapon.weapons[weaponIndex].maxAmmo) return;
            
            onCollected?.Invoke();
            other.GetComponent<PlayerWeapon>().AddAmmo(weaponIndex, amount);
            this.gameObject.SetActive(false);
        }
    }
}
