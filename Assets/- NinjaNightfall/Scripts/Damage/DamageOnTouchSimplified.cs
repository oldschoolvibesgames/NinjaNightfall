using MoreMountains.CorgiEngine;
using MoreMountains.Feedbacks;
using UnityEngine;

public class DamageOnTouchSimplified : MonoBehaviour
{
    [Header("Damage Settings")]
    public LayerMask targetLayers;
    public float damageAmount = 10f;
    public float invincibilityDuration = 0.5f;
    public bool isDistanceAttack;

    [Header("Feedbacks")]
    [Tooltip("Feedback quando atinge algo com Health")]
    public MMFeedbacks HitDamageableFeedback;
    
    [Tooltip("Feedback quando atinge algo sem Health")]
    public MMFeedbacks HitNonDamageableFeedback;

    private void Awake()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.isTrigger = true;
        }

        // Inicializa os feedbacks
        HitDamageableFeedback?.Initialization(this.gameObject);
        HitNonDamageableFeedback?.Initialization(this.gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryApplyDamage(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        TryApplyDamage(other);
    }

    private void TryApplyDamage(Collider2D other)
    {
        if (!enabled) return;
        if (!IsInLayerMask(other.gameObject.layer, targetLayers)) return;

        var health = other.GetComponent<Health>();

        if (health != null && health.CurrentHealth > 0 && health.CanTakeDamageThisFrame())
        {
            CharacterManager character = health.gameObject.GetComponent<CharacterManager>();
            if (character != null)
            {
                if (character.canBlockDistanceAttack && isDistanceAttack)
                {
                    character.BlockAttack();
                    HitNonDamageableFeedback?.PlayFeedbacks(transform.position);
                }
                else
                {
                    Vector2 direction = (other.transform.position - transform.position).normalized;
                    health.Damage(damageAmount, gameObject, invincibilityDuration, invincibilityDuration, direction);

                    // ➕ Feedback de acerto em alvo com Health
                    HitDamageableFeedback?.PlayFeedbacks(transform.position);
                }
            }
        }
        else
        {
            // ➕ Feedback de acerto em alvo sem Health
            HitNonDamageableFeedback?.PlayFeedbacks(transform.position);
        }

        this.gameObject.SetActive(false);
    }

    private bool IsInLayerMask(int layer, LayerMask mask)
    {
        return (mask.value & (1 << layer)) != 0;
    }
}
