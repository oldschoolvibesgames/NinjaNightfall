using MoreMountains.CorgiEngine;
using UnityEngine;

public class WallShooter : MonoBehaviour
{
    public GameObject bulletPrefab;
    public Transform shootPoint;
    public Transform aimTarget;
    public int ammo = 5;
    public bool infiniteAmmo = false;

    private int _currentAmmo;

    private void Awake()
    {
        _currentAmmo = ammo;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (!infiniteAmmo && _currentAmmo <= 0) return;

        Shoot();

        if (!infiniteAmmo)
        {
            _currentAmmo--;
        }
    }

    public void ResetAmmo()
    {
        _currentAmmo = ammo;
    }

    private void Shoot()
    {
        var bullet = Instantiate(bulletPrefab, shootPoint.position, shootPoint.rotation);

        Vector3 direction = (aimTarget.position - shootPoint.position).normalized;

        var projectile = bullet.GetComponent<Projectile>();
        if (projectile != null)
        {
            projectile.Direction = direction;
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (shootPoint != null && aimTarget != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(shootPoint.position, aimTarget.position);
            Gizmos.DrawSphere(aimTarget.position, 0.1f);
        }
    }
#endif
}