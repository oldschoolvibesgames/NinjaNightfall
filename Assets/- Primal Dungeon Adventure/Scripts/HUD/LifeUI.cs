using UnityEngine;
using UnityEngine.UI;

public class LifeUI : MonoBehaviour
{
    [SerializeField]private Sprite _lifeOff;
    [SerializeField]private Sprite _lifeOn;
    private Image _thisImage;

    public void SetLife(bool isOn)
    {
        if(_thisImage == null) _thisImage = GetComponent<Image>();
        if (isOn) _thisImage.sprite = _lifeOn;
        else _thisImage.sprite = _lifeOff;
    }
}
