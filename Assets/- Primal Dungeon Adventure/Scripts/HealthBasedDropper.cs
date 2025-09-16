using UnityEngine;
using MoreMountains.CorgiEngine;

public class HealthBasedDropper : MonoBehaviour
{
    [System.Serializable]
    public class DropItem
    {
        public float triggerHealth;
        public GameObject[] itemsToDrop;
        [HideInInspector] public bool hasDropped = false;
    }

    [Header("Referências")]
    public Health characterHealth;

    [Header("Itens de Drop")]
    public DropItem[] dropItems;

    private void Awake()
    {
        if (characterHealth == null)
        {
            characterHealth = GetComponent<Health>();
        }
    }

    private void OnEnable()
    {
        foreach (var drop in dropItems)
        {
            drop.hasDropped = false;
        }
    }

    private void Update()
    {
        foreach (var drop in dropItems)
        {
            if (!drop.hasDropped && characterHealth.CurrentHealth <= drop.triggerHealth)
            {
                foreach (var item in drop.itemsToDrop)
                {
                    Instantiate(item, transform.position, Quaternion.identity);
                }

                drop.hasDropped = true;
            }
        }
    }
}