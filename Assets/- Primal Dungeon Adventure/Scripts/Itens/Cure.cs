using MoreMountains.CorgiEngine;
using UnityEngine;
using UnityEngine.Events;

public class Cure : MonoBehaviour
{
    public int value;
    public UnityEvent onCollect;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.CompareTag("Player"))
        {
            var health = other.GetComponent<Health>();
            if(health.CurrentHealth >= health.MaximumHealth) return;
            
            onCollect?.Invoke();
            other.GetComponent<PlayerHealth>().Haling(value);
            this.gameObject.SetActive(false);
        }
    }
}
