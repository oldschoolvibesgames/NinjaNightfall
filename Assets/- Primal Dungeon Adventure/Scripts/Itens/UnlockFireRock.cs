using UnityEngine;

public class UnlockFireRock : MonoBehaviour
{
    public int weaponIndex;
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.CompareTag("Player"))
        {
            var weapon = other.GetComponent<PlayerWeapon>();
            weapon.weapons[weaponIndex].unlocked = true;
            //this.gameObject.SetActive(false);
        }
    }
}
