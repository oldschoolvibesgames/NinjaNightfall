using UnityEngine;

public class TopDownTrap : MonoBehaviour
{
    public Rigidbody2D rg2d;
    private Vector3 _startPosition;
    private bool _disarmed;

    private void Awake()
    {
        rg2d.bodyType = RigidbodyType2D.Kinematic;
        _startPosition = transform.localPosition;
    }

    private void OnEnable()
    {
        rg2d.bodyType = RigidbodyType2D.Kinematic;
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if(_disarmed) return;
        if (col.CompareTag("Player"))
        {
            rg2d.bodyType = RigidbodyType2D.Dynamic;
            _disarmed = true;
        }
    }

    public void ResetTrap()
    {
        rg2d.linearVelocity = Vector2.zero;
        rg2d.angularVelocity = 0f;
        rg2d.bodyType = RigidbodyType2D.Kinematic;
        transform.localPosition = _startPosition;
        _disarmed = false;
    }

}
