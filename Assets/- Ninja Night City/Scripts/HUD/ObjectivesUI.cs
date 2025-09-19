using UnityEngine.UI;
using UnityEngine;

public class ObjectivesUI : MonoBehaviour
{
    public Image[] images;
    public Sprite off;
    public Sprite on;

    public void UpdateInfo(int objectivesDone)
    {
        for (int i = 0; i < images.Length; i++)
        {
            if (i < objectivesDone) images[i].sprite = on;
            else images[i].sprite = off;
        }
    }
}
