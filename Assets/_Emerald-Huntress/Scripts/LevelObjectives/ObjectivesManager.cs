using UnityEngine;
using UnityEngine.Events;

public class ObjectivesManager : MonoBehaviour
{
    public UnityEvent onAllDone;
    
    private LevelObjective[] _objectives;
    //private int _objectivesCollected;
    private int _objectivesDone = 0;
    private ObjectivesUI _objectivesUI;

    private void Awake()
    {
        _objectives = FindObjectsByType<LevelObjective>(0);

        foreach (var objective in _objectives)
        {
            objective.onObjectiveCollected += ObjectiveCollected;
        }

        _objectivesUI = FindAnyObjectByType<ObjectivesUI>();
        UpdateUI();
    }

    public void ObjectiveCollected()
    {
        _objectivesDone++;
        UpdateUI();
        CheckAllDone();
    }

    private void CheckAllDone()
    {
        if(_objectivesDone >= _objectives.Length) onAllDone?.Invoke();
    }

    private void UpdateUI()
    {
        if (_objectivesUI != null)
        {
            _objectivesUI.UpdateInfo(_objectivesDone);
        }
    }
}
