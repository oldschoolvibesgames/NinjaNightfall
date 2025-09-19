using MoreMountains.CorgiEngine;
using UnityEngine;

public class Damage : MonoBehaviour
{
    public int value;
    public float invincibilityDuration = 1f;
    public float flickerDuration = 1f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.CompareTag("Player"))
        {
            other.GetComponent<Health>().Damage(value, this.gameObject, flickerDuration, invincibilityDuration, this.transform.right);
        }
    }
}
