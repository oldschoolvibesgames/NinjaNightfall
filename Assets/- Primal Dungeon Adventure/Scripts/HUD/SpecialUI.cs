using UnityEngine;
using UnityEngine.UI;

public class SpecialUI : MonoBehaviour
{
    public GameObject[] contents;
    public Image filled;

    public void IsUnlocked(bool isUnlocked)
    {
        if (isUnlocked)
        {
            foreach (var i in contents)
            {
                if(!i.activeSelf) i.SetActive(true);
            }
        }
        else
        {
            foreach (var i in contents)
            {
                if(i.activeSelf) i.SetActive(false);
            }
        }
    }
    
    public void SetFilled(float value)
    {
        filled.fillAmount = value;
    }
}
