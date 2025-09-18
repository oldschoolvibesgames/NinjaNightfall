using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UnlockAchievement : MonoBehaviour
{
    [SerializeField] private string achievementId;

    private void Start()
    {

    }

    private void OnDestroy()
    {
        
    }

    public void Unlock()
    {
        if (achievementId == null || achievementId.Length == 0)
        {
            string sceneName = SceneManager.GetActiveScene().name;
            achievementId = Regex.Replace(sceneName, "[^0-9.]", "");
        }

        GDKAchievements.Instance.UnlockAchievement(achievementId);
    }
}