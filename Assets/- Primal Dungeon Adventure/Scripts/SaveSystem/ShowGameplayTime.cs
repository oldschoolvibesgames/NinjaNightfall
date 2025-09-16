using TMPro;
using UnityEngine;

public class ShowGameplayTime : MonoBehaviour
{
    public GameData gameData;
    private TMP_Text _thisText;

    private void Start()
    {
        _thisText = GetComponent<TMP_Text>();

        // Supondo que playerGameTime seja em segundos (float ou int)
        int totalSeconds = Mathf.FloorToInt(gameData.playerGame.playerGameTime);

        int hours = totalSeconds / 3600;
        int minutes = (totalSeconds % 3600) / 60;
        int seconds = totalSeconds % 60;

        _thisText.text = string.Format("{0:D2}:{1:D2}:{2:D2}", hours, minutes, seconds);
    }
}
