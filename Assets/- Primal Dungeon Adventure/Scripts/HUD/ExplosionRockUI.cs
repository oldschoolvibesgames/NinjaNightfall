using UnityEngine;
using UnityEngine.UI;

public class ExplosionRockUI : MonoBehaviour
{
    public Image filled;
    public Image icon;
    public Color colorOff;
    public int indexWeapon;
    public GameObject[] contents;
    private Color _colorOn;
    private PlayerWeapon _playerWeapon;

    private void Awake()
    {
        _colorOn = icon.color;
    }

    private void Update()
    {
        if (_playerWeapon == null) _playerWeapon = FindAnyObjectByType<PlayerWeapon>();

        if (_playerWeapon != null)
        {
            if (_playerWeapon.weapons[indexWeapon].unlocked)
            {
                if (!icon.gameObject.activeSelf)
                {
                    icon.gameObject.SetActive(true);
                    foreach (var i in contents)
                    {
                        i.SetActive(true);
                    }
                }

                if (_playerWeapon.weapons[indexWeapon].ammo > 0)
                {
                    icon.color = _colorOn;
                }
                else
                {
                    icon.color = colorOff;
                }
            }
            else
            {
                if (icon.gameObject.activeSelf)
                {
                    icon.gameObject.SetActive(false);
                    foreach (var i in contents)
                    {
                        i.SetActive(false);
                    }
                }
            }

            float resultKills = (float)_playerWeapon.weapons[1].countKills / 3.0f;
            if (resultKills > 1) resultKills = 1;
            filled.fillAmount = resultKills;
        }
    }
}
