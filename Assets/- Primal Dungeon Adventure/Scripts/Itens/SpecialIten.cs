using System;
using UnityEngine;

public class SpecialIten : MonoBehaviour
{
    public int intenIndex;
    private PlayerData _playerData;

    private void Update()
    {
        _playerData = FindAnyObjectByType<PlayerData>();
        if(_playerData.IsItemCollected(intenIndex)) this.gameObject.SetActive(false);
    }

    private void OnEnable()
    {

    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.CompareTag("Player"))
        {
            _playerData.SetItemCollected(intenIndex, true);
            this.gameObject.SetActive(false);
        }
    }
}
