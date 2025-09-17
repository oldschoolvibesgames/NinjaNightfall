using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelSystem : MonoBehaviour
{
    public Level[] levels;
    public GameData gameData;
    public Animator transition;

    private void Start()
    {
       transition.Play("Out");
    }

    private void OnEnable()
    {
        transition.Play("Out");
    }

    public void GoToNextLevel()
    {
        string currentLevel = SceneManager.GetActiveScene().name;
        int nextLevel = Int32.Parse(currentLevel) + 1;
        
        //int nextLevel = Int32.Parse(gameData.playerGame.currentLevel) + 1;
        string nextLevelName = nextLevel.ToString();
        bool hasLevel = false;


        foreach (var lvl in levels)
        {
            if (lvl.levelName == nextLevelName) 
            {
                hasLevel = true;
                UnlockLevel(nextLevelName);//
                GoToLevel(nextLevelName);
            }
        }
        if (!hasLevel)
        {
            Transition();
            SceneManager.LoadSceneAsync("EndGame");
        }
    }

    public void GoToLevel(string levelName)
    {
        if (SceneManager.GetActiveScene().name == levelName)
        {
            gameData.playerGame.currentLevel = levelName;
            gameData.SaveGame();
        }
        else
        {
            gameData.playerGame.currentLevel = levelName;
            gameData.SaveGame();
            Transition();
            SceneManager.LoadSceneAsync(levelName);
        }
    }

    public void GoToLevelWithScene(string levelName)
    {
        gameData.playerGame.currentLevel = levelName;
        gameData.SaveGame();
        Transition();
        SceneManager.LoadSceneAsync(levelName);
    }

    public void LoadCurrentLevel()
    {
        GoToLevel(gameData.playerGame.currentLevel);
    }

    public void GoToStory(string levelName)
    {
        gameData.playerGame.currentLevel = levelName;
        gameData.SaveGame();
        Transition();
        SceneManager.LoadSceneAsync(levelName);
    }

    public void OnLevelCompleted(string levelNameCompleted)
    {
        foreach (var gameLevel in gameData.playerGame.levels)
        {
            if (gameLevel.levelName == levelNameCompleted)
            {
                gameLevel.isDone = true;
                int nextLevel = Int32.Parse(gameLevel.levelName) + 1;
                UnlockLevel(nextLevel.ToString());
                gameData.SaveGame();
                break;
            }
        }
    }

    void UnlockLevel(string levelName)
    {
        foreach (var l in gameData.playerGame.levels)
        {
            if (l.levelName == levelName)
            {
                l.isLocked = false;
            }
        }
    }

    public void UpdateLevels()
    {
        foreach (var lvl in levels)
        {
            bool hasLevel = false;

            foreach (var gameLevel in gameData.playerGame.levels)
            {
                if (gameLevel.levelName == lvl.levelName)
                {
                    hasLevel = true;
                    lvl.isLocked = gameLevel.isLocked;
                    lvl.isDone = gameLevel.isDone;
                    break;
                }
            }

            if (!hasLevel)
            {
                GameData.LevelStatus newLevel = new GameData.LevelStatus();
                newLevel.ConstructLevel(lvl.levelName, lvl.isLocked, lvl.isDone);
                gameData.playerGame.levels.Add(newLevel);
            }
        }

        gameData.SaveGame();
    }

    private void Transition()
    {
        transition.Play("In");
    }

    [Serializable]
    public class Level
    {
        public string levelName;
        public bool isLocked;
        public bool isDone;
    }
}
