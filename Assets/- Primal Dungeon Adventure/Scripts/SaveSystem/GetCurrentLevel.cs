using TMPro;
using UnityEngine;

public class GetCurrentLevel : MonoBehaviour
{
    private TMP_Text _thisText;
    private GameData _gameData;
    
    void Awake()
    {
        _thisText = GetComponent<TMP_Text>();
        _gameData = FindObjectOfType<GameData>();
    }
    
    void Update()
    {
        _thisText.text = _gameData.playerGame.currentLevel;
    }
}
