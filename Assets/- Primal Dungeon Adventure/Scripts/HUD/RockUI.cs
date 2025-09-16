using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RockUI : MonoBehaviour
{
    public Image icon;
    public TMP_Text ammoText;
    public int indexWeapon;
    private int _maxAmmo;
    private int _actualAmmo;
    private PlayerWeapon _playerWeapon;
    
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
                    ammoText.gameObject.SetActive(true);
                }
                
                _maxAmmo = _playerWeapon.weapons[indexWeapon].maxAmmo;
                _actualAmmo = _playerWeapon.weapons[indexWeapon].ammo;
                UpdateUI();
            }
            else
            {
                if (icon.gameObject.activeSelf)
                {
                    icon.gameObject.SetActive(false);
                    ammoText.gameObject.SetActive(false);
                }
            }
        }
    }

    private void UpdateUI()
    {
        ammoText.text = _actualAmmo + " / " + _maxAmmo;
    }
}
