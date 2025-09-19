using System;
using MoreMountains.CorgiEngine;
using UnityEngine;

public class WeaponMelee : MonoBehaviour
{
    [Header("Layers")]
    public LayerMask layersToCollider;
    public LayerMask targetDamage;

    [Header("Refs")]
    [SerializeField] private Rigidbody2D _rigidBody;
    [SerializeField] private Collider2D _collider2D;

    public int damage;

    /*private void OnCollisionEnter2D(Collision2D col)
    {
        Debug.Log(col.gameObject.name);
        int colLayer = col.gameObject.layer;
        int layerMaskCollided = 1 << colLayer;

        if ((layersToCollider.value & layerMaskCollided) != 0)
        {
            if ((targetDamage.value & layerMaskCollided) != 0)
            {
                Debug.Log("Enemy Collided");
                var health = col.gameObject.GetComponent<Health>();
                if (health != null)
                {
                    health.Damage(damage, gameObject, 1f, 1f, transform.right);
                    if (health.CurrentHealth <= 0)
                    {
                        //FindAnyObjectByType<KillsManager>().AddKillByWeapon(_weaponIndex);
                    }
                }
            }
        }
    }*/
    
    private void OnTriggerEnter2D(Collider2D col)
    {
        Debug.Log(col.gameObject.name);
        int colLayer = col.gameObject.layer;
        int layerMaskCollided = 1 << colLayer;

        if ((layersToCollider.value & layerMaskCollided) != 0)
        {
            if ((targetDamage.value & layerMaskCollided) != 0)
            {
                Debug.Log("Enemy Collided");
                var health = col.gameObject.GetComponent<Health>();
                if (health != null)
                {
                    health.Damage(damage, gameObject, 1f, 1f, transform.right);
                    if (health.CurrentHealth <= 0)
                    {
                        //FindAnyObjectByType<KillsManager>().AddKillByWeapon(_weaponIndex);
                    }
                }
            }
        }
    }
}
