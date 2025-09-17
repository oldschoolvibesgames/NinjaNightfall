using UnityEngine;

public class UnlockSpecialPrimal : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.CompareTag("Player"))
        {
            var weapon = other.GetComponent<SpecialAreaAttack>();
            weapon.unlocked = true;
            //this.gameObject.SetActive(false);
        }
    }
}
