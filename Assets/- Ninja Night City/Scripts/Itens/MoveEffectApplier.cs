using UnityEngine;

public class MoveEffectApplier : MonoBehaviour
{
    public float effectDuration = 2f;
    public Color effectColor = Color.green;
    public bool destroyAfterApply = true;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            var moveEffect = other.GetComponent<MoveEffect>();
            if (moveEffect != null)
            {
                moveEffect.ApplyMoveReduction(effectDuration, effectColor);

                if (destroyAfterApply)
                {
                    Destroy(gameObject);
                }
            }
        }
    }
}