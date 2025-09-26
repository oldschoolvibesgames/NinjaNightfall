using System;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class CountdownStandard
{
    public float maxTime;
    public UnityEvent onTimeEnd;
    private float _actualTime;
    private bool _countTimer;
    
    public void UpdateClass()
    {
        if (_countTimer)
        {
            if (_actualTime > 0) _actualTime -= Time.deltaTime;
            else
            {
                _countTimer = false;
                onTimeEnd?.Invoke();
            }
        }
    }

    public void StartTimer()
    {
        _countTimer = true;
        _actualTime = maxTime;
    }
}
