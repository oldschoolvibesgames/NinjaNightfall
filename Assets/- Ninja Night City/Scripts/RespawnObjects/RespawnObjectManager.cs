using UnityEngine;

public class RespawnObjectManager : MonoBehaviour
{
    private RespawnObject[] _respawnObjects;

    private void Awake()
    {
        _respawnObjects = FindObjectsByType<RespawnObject>(0);
    }

    public void RespawnObj()
    {
        foreach (var obj in _respawnObjects)
        {
            if (obj.isRespawnable)
            {
                obj.gameObject.SetActive(true);
                obj.OnReset();
            }
        }
    }
}
