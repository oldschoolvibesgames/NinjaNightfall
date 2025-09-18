using System.Collections.Generic;
using UnityEngine;
using MoreMountains.CorgiEngine;

public class ProjectileShooterPool : MonoBehaviour
{
    [Header("Configurações do Pool")]
    public Projectile projectilePrefab;
    public int poolSize = 10;

    [Header("Configuração de Disparo")]
    public Transform shootPivot; // <- NOVO: posição/orientação do tiro

    private List<Projectile> _projectilePool = new List<Projectile>();
    private Transform _poolParent;

    private void Awake()
    {
        // Criar objeto pai para armazenar projéteis desativados
        _poolParent = new GameObject($"{name}_ProjectilePool").transform;
        _poolParent.SetParent(this.transform);

        // Preencher o pool
        for (int i = 0; i < poolSize; i++)
        {
            Projectile newProjectile = Instantiate(projectilePrefab, _poolParent);
            newProjectile.gameObject.SetActive(false);
            _projectilePool.Add(newProjectile);
        }
    }

    /// <summary>
    /// Dispara um projétil na direção e posição do shootPivot.
    /// </summary>
    public void Shot()
    {
        if (shootPivot == null)
        {
            Debug.LogWarning("Shoot Pivot não atribuído!");
            return;
        }

        Projectile projectileToFire = GetPooledProjectile();

        if (projectileToFire != null)
        {
            // Remove do pool temporariamente
            projectileToFire.transform.SetParent(null);

            // Define posição e rotação conforme o pivot
            projectileToFire.transform.position = shootPivot.position;
            projectileToFire.transform.rotation = shootPivot.rotation;

            // Define a direção com base no vetor local do pivot
            Vector3 shootDirection = shootPivot.right;
            projectileToFire.SetDirection(shootDirection, shootPivot.rotation);

            // Ativa o projétil
            projectileToFire.gameObject.SetActive(true);

            // Monitora quando o projétil for desativado para retornar ao pool
            StartCoroutine(WaitForDisable(projectileToFire));
        }
        else
        {
            Debug.LogWarning("Sem projéteis disponíveis no pool!");
        }
    }

    private Projectile GetPooledProjectile()
    {
        foreach (var projectile in _projectilePool)
        {
            if (!projectile.gameObject.activeInHierarchy)
            {
                return projectile;
            }
        }
        return null;
    }

    private System.Collections.IEnumerator WaitForDisable(Projectile projectile)
    {
        yield return new WaitUntil(() => !projectile.gameObject.activeInHierarchy);
        projectile.transform.SetParent(_poolParent);
    }
}
