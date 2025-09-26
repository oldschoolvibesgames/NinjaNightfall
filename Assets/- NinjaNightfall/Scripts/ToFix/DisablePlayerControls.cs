using System;
using MoreMountains.CorgiEngine;
using UnityEngine;

public class DisablePlayerControls : MonoBehaviour
{
    private Character _player;
    
    private void OnEnable()
    {
        if(_player == null) _player = GameObject.FindWithTag("Player").GetComponent<Character>();
        _player.enabled = false;
    }

    private void OnDisable()
    {
        if(_player == null) _player = GameObject.FindWithTag("Player").GetComponent<Character>();
        _player.enabled = true;
    }
}
