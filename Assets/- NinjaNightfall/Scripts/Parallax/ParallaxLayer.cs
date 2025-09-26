using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class ParallaxLayer : MonoBehaviour
{
    public float parallaxSpeed = 0.5f;
    public bool affectY = false; 

    private Transform _cameraTransform;
    private Vector3 _lastCameraPosition;
    private float _spriteWidth;
    private SpriteRenderer _spriteRenderer;

    private void Start()
    {
        _cameraTransform = Camera.main.transform;
        _lastCameraPosition = _cameraTransform.position;
        _spriteRenderer = GetComponent<SpriteRenderer>();

        _spriteWidth = _spriteRenderer.bounds.size.x;

        
        for (int i = -1; i <= 1; i++)
        {
            if (i == 0) continue;
            GameObject clone = Instantiate(gameObject, transform.position + Vector3.right * _spriteWidth * i, transform.rotation, transform);
            Destroy(clone.GetComponent<ParallaxLayer>());
        }
    }

    private void LateUpdate()
    {
        Vector3 deltaMovement = _cameraTransform.position - _lastCameraPosition;
        
        float moveX = deltaMovement.x * parallaxSpeed;
        float moveY = affectY ? deltaMovement.y * parallaxSpeed : 0f;

        transform.position += new Vector3(moveX, moveY, 0f);
        _lastCameraPosition = _cameraTransform.position;
        
        float cameraX = _cameraTransform.position.x;
        float diff = cameraX - transform.position.x;

        if (Mathf.Abs(diff) >= _spriteWidth)
        {
            float offset = (diff > 0) ? _spriteWidth : -_spriteWidth;
            transform.position += Vector3.right * offset;
        }
    }
}
