using UnityEngine;
using MoreMountains.CorgiEngine;
using System.Collections;

[RequireComponent(typeof(Collider2D))]
public class KnockbackTrigger : MonoBehaviour
{
    public float horizontalDistance = 2f;
    public float verticalDistance = 1f;
    public float duration = 0.2f;
    public float disableDuration = 0.3f;
    public LayerMask obstacleLayer;

    private void Reset()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        var playerHealth = other.GetComponent<Health>();
        if(playerHealth.CurrentHealth <= 0) return;
        
        var movement = other.GetComponent<CharacterHorizontalMovement>();
        var runner = other.GetComponent<MonoBehaviour>();

        if (movement != null && runner != null)
        {
            Vector3 direction = (other.transform.position.x < transform.position.x) ? Vector3.right : Vector3.left;
            Vector3 startPos = other.transform.position;
            Vector3 endPos = startPos + direction * horizontalDistance + Vector3.up * verticalDistance;

            movement.MovementForbidden = true;

            runner.StartCoroutine(SmoothMoveWithCollision(other.transform, startPos, endPos, duration, obstacleLayer));
            runner.StartCoroutine(ReenableMovement(movement, disableDuration));
        }
    }

    private IEnumerator SmoothMoveWithCollision(Transform target, Vector3 from, Vector3 to, float duration, LayerMask obstacleLayer)
    {
        float time = 0f;
        Vector3 lastPosition = from;

        while (time < duration)
        {
            Vector3 nextPosition = Vector3.Lerp(from, to, time / duration);

            Vector2 checkSize = new Vector2(0.5f, 1f); // Ajuste conforme o tamanho do player
            Collider2D hit = Physics2D.OverlapBox(nextPosition, checkSize, 0f, obstacleLayer);

            if (hit != null)
                break;

            target.position = nextPosition;
            lastPosition = nextPosition;

            time += Time.deltaTime;
            yield return null;
        }

        target.position = lastPosition;
    }

    private IEnumerator ReenableMovement(CharacterHorizontalMovement movement, float delay)
    {
        yield return new WaitForSeconds(delay);
        movement.MovementForbidden = false;
    }
}
