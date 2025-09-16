using TMPro;
using UnityEngine;

public class LevelInfo : MonoBehaviour
{
    public TMP_Text levelText;
    public string levelName;

    private void Awake()
    {
        levelText.text = levelName;
    }
}
