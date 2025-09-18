using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BoxLevel : MonoBehaviour
{
    public int level;
    public string historyScene;
    public Button button;
    public TMP_Text levelText;
    public GameObject lockImage;
    public GameObject doneImage;
    
    private GameData _gameData;
    private LevelSystem _levelSystem;
    private GameData.LevelStatus _thisLevel;

    private void Awake()
    {
        _gameData = FindObjectOfType<GameData>();
        button.onClick.AddListener(OnClick);
        _levelSystem = FindObjectOfType<LevelSystem>();
    }

    private void OnEnable()
    {
        _thisLevel = _gameData.playerGame.levels[level - 1];
        UpdateStatus();
    }

    void UpdateStatus()
    {
        levelText.text = level.ToString();
        lockImage.SetActive(_thisLevel.isLocked);
        doneImage.SetActive(_thisLevel.isDone);

        if (_thisLevel.isLocked)
        {
            doneImage.SetActive(false);
            levelText.gameObject.SetActive(false);
            button.gameObject.SetActive(false);
        }
        else
        {
            levelText.gameObject.SetActive(true);
            button.gameObject.SetActive(true);
        }
    }

    public void OnClick()
    {
        string levelName = level.ToString();
        
        if (string.IsNullOrEmpty(historyScene))
        {
            _levelSystem.GoToLevelWithScene(levelName);
        }
        else
        {
            _levelSystem.GoToStory(historyScene); // ← Corrigido aqui
        }
    }
}