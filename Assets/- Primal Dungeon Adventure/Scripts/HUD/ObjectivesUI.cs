using TMPro;
using UnityEngine;

public class ObjectivesUI : MonoBehaviour
{
    public TMP_Text info;

    public void UpdateInfo(string newInfo)
    {
        info.text = newInfo;
    }
}
