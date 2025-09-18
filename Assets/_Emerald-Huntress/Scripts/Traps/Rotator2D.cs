using UnityEngine;

public class Rotator2D : MonoBehaviour
{
    public float rotationSpeed = 100f;
    public bool clockwise = true;

    void Update()
    {
        float direction = clockwise ? -1f : 1f;
        transform.Rotate(0f, 0f, direction * rotationSpeed * Time.deltaTime);
    }
}