using TMPro;
using UnityEngine;

public class GDKUserName : MonoBehaviour
{
    [SerializeField]
    string prefix;

    [SerializeField]
    string suffix;

#if MICROSOFT_GDK_SUPPORT || UNITY_GAMECORE
    TextMeshProUGUI gamertagText;

    void Start()
    {
        gamertagText = GetComponent<TextMeshProUGUI>();
        SetGamertag();
    }

    private void SetGamertag()
    {
        string message = "{0}{1}{2}";
        gamertagText.text = string.Format(message, prefix, GDKUserInfo.gamertag, suffix);
    }
#endif
}
