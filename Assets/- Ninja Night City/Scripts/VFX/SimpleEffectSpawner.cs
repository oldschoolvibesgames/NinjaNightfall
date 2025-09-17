using UnityEngine;

public class SimpleEffectSpawner : MonoBehaviour
{
    [Header("Efeitos")]
    public AudioSource audioSource;
    public GameObject[] particlePrefab;

    [Header("Auto Spawn")]
    public bool spawnOnDisableOrDestroy = false;

    private bool _alreadySpawned = false;

    /// <summary>
    /// Instancia as partículas e toca o áudio no local indicado.
    /// </summary>
    /// <param name="position">Local onde o efeito deve ocorrer</param>
    public void Spawn(Vector3 position, int index)
    {
        if (particlePrefab != null)
        {
            Instantiate(particlePrefab[index], position, Quaternion.identity);
        }

        if (audioSource != null)
        {
            audioSource.Play();
        }

        _alreadySpawned = true;
    }

    private void OnDisable()
    {
        if (spawnOnDisableOrDestroy && !_alreadySpawned)
        {
            Spawn(transform.position, 0);
        }
    }
}