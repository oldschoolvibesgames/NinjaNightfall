using System;
using UnityEngine;
using UnityEngine.Events;

public class FakePlataform : MonoBehaviour
{
    public GameObject plataform;
    public float timerToDisable = 0.2f;
    public float timerToRespawn = 2f;
    public UnityEvent onDisable;
    public Rigidbody2D rgbody2D;
    public EdgeCollider2D collider;
    private float _actualTimer;
    private bool _countTimer;
    private float _timerToRespawn;
    private bool _countRespawn;
    private Vector3 _startPosition;

    private void Awake()
    {
        _startPosition = transform.position;
    }

    private void OnEnable()
    {
        ResetObject(); 
    }

    private void Update()
    {
        if (_countTimer)
        {
            if (_actualTimer > 0)
            {
                _actualTimer -= Time.deltaTime;
            }
            else
            {
                _countTimer = false;
                onDisable?.Invoke();
                // A plataforma cai
                rgbody2D.bodyType = RigidbodyType2D.Dynamic;
                collider.enabled = false;

                _timerToRespawn = timerToRespawn;
                _countRespawn = true;
            }
        }
        
        if(_countRespawn)
        {
            RespawnTimer();
        }
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Feet"))
        {
            _countTimer = true;
        }
    }

    private void RespawnTimer()
    {
        if (_timerToRespawn > 0)
        {
            _timerToRespawn -= Time.deltaTime;
        }
        else
        {
            ResetObject();
        }
    }

    public void ResetObject()
    {
        _actualTimer = timerToDisable;
        rgbody2D.linearVelocity = Vector2.zero;
        rgbody2D.angularVelocity = 0f;
        rgbody2D.bodyType = RigidbodyType2D.Kinematic;
        collider.enabled = true;
        _countRespawn = false;
        transform.position = _startPosition;
    }
}