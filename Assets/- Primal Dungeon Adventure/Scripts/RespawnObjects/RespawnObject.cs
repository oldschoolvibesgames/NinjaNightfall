using System;
using MoreMountains.CorgiEngine;
using UnityEngine;
using UnityEngine.Events;

public class RespawnObject : MonoBehaviour
{
    public bool isRespawnable;
    public bool isEnemy;
    public UnityEvent onReset;

    private Vector3 _originalPosition;

    private void Awake()
    {
        _originalPosition = transform.localPosition;
    }

    public void OnReset()
    {
        if(isEnemy) gameObject.GetComponent<Health>().Revive();
        transform.localPosition = _originalPosition;
        
        onReset?.Invoke();
    }
}
