using System;
using UnityEngine;

public class LevelObjective : MonoBehaviour
{
    public Action onObjectiveCollected;
    private bool _objectiveDone;

    private void OnTriggerEnter2D(Collider2D col)
    {
        if(_objectiveDone) return;
        
        if (col.CompareTag("Player"))
        {
            _objectiveDone = true;
            onObjectiveCollected?.Invoke();
        }
    }
}
