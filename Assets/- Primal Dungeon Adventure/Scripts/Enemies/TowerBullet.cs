using MoreMountains.CorgiEngine;
using UnityEngine;

public class TowerBullet : MonoBehaviour
{
    public float lifeTime = 5f;
    public int damage;
    private void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            collision.GetComponent<Health>().Damage(damage, this.gameObject, 1f, 1f, this.transform.right);
            Destroy(gameObject);
        }

        if (!collision.isTrigger && !collision.CompareTag("Enemy"))
        {
            Destroy(gameObject);
        }
    }
}