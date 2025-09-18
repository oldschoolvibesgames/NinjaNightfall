using UnityEngine;

public class NextLevelTrigger : MonoBehaviour
{
    private LevelSystem _levelSystem;
    private bool _done;
    [SerializeField] private string achievementId;

    private void Awake()
    {
        _levelSystem = FindAnyObjectByType<LevelSystem>();
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if(_done) return;

        if (col.CompareTag("Player"))
        {
            _done = true;
            FindAnyObjectByType<PlayerData>().SaveAll();
            GDKAchievements.Instance.UnlockAchievement(achievementId);
            _levelSystem.GoToNextLevel();
        }
    }
}
