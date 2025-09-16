using UnityEngine;

public class JumpEffectApplier : MonoBehaviour
{
    public float effectDuration = 5f;
    public Color effectColor = Color.yellow;
    public bool destroyAfterApply = true;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            var jumpEffect = other.GetComponent<JumpEffect>();
            if (jumpEffect != null)
            {
                jumpEffect.ApplyJumpReduction(effectDuration, effectColor);

                if (destroyAfterApply)
                {
                    Destroy(gameObject);
                }
            }
        }
    }
}