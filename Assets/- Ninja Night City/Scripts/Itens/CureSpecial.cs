using UnityEngine;
using UnityEngine.Events;

public class CureSpecial : MonoBehaviour
{
    public UnityEvent onCollect;
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.CompareTag("Player"))
        {
            onCollect?.Invoke();
            other.GetComponent<PlayerHealth>().AddLifeSlot();
            this.gameObject.SetActive(false);
        }
    }
}
