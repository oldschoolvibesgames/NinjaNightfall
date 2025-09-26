using UnityEngine;

public class KillCount : MonoBehaviour
{
    private void OnDisable()
    {
        var killsManager = FindAnyObjectByType<KillsManager>();
        if(killsManager != null) killsManager.AddKill();
    }
}
