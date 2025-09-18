using UnityEngine;
using UnityEngine.UI;

public class LifeUI : MonoBehaviour
{
    public Image[] images;
    public Sprite lifeOff;
    public Sprite lifeOn;

    public void UpdateUI(int currentLife, int maxLife)
    {
        for (int i = 0; i < images.Length; i++)
        {
            if (i < maxLife)
            {
                images[i].gameObject.SetActive(true);
                if (i < currentLife) images[i].sprite = lifeOn;
                else images[i].sprite = lifeOff;
            }
            else
            {
                images[i].gameObject.SetActive(false);
            }
        }
    }
}
