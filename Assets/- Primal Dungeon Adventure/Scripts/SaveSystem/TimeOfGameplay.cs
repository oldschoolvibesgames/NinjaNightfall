using System;
using UnityEngine;

public class TimeOfGameplay : MonoBehaviour
{
    public GameObject pause;
    private bool _countTime = false;
    private float _actualTime = 0f;

    private void Update()
    {
        if (!pause.activeSelf && _countTime) _actualTime += Time.deltaTime;
    }

    public void CountTimeStatus(bool canCount)
    {
        _countTime = canCount;
    }

    public void SetTimeInGame(GameData gameData)
    {
        gameData.playerGame.playerGameTime += _actualTime;
        _actualTime = 0f;
    }
}
