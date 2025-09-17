using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWeapon : MonoBehaviour
{
    public Transform orientation;
    public int index;
    public Weapon[] weapons;

    private PlayerInputs _playerInputs;
    private GamePause _gamePause;

    private void Awake()
    {
        _playerInputs = new PlayerInputs();
        _playerInputs.Gameplay.Enable();

        _gamePause = FindAnyObjectByType<GamePause>();

        foreach (var weapon in weapons)
        {
            weapon.InitializePool(transform);
        }

        weapons[0].unlocked = true;
    }

    private void Update()
    {
        if (_gamePause.IsPaused()) return;

        foreach (var weapon in weapons)
        {
            weapon.Update();
        }

        if (_playerInputs.Gameplay.Attack0.triggered)
        {
            Fire(0);
        }

        if (_playerInputs.Gameplay.Attack1.triggered)
        {
            if (weapons[2].ammo > 0 && weapons[2].unlocked) Fire(2);
            else Fire(1);
        }
    }

    public void ClearKill(int weaponIndex)
    {
        weapons[weaponIndex].ClearKills();
        weapons[2].ammo = 0;
    }

    public void Fire(int newIndex)
    {
        if (!weapons[newIndex].unlocked) return;

        if (newIndex == 2) weapons[1].ClearKills();
        weapons[newIndex].Fire(orientation, newIndex);
    }

    public void AddAmmo(int weaponIndex, int amount)
    {
        weapons[weaponIndex].ammo += amount;
        if (weapons[weaponIndex].ammo > weapons[weaponIndex].maxAmmo)
        {
            weapons[weaponIndex].ammo = weapons[weaponIndex].maxAmmo;
        }
    }

    public void ChangeWeapon()
    {
        index++;
        if (index >= weapons.Length) index = 0;
        if (!weapons[index].unlocked) ChangeWeapon();
    }

    public void RegisterKill(int weaponIndex)
    {
        weapons[weaponIndex].countKills++;
        if (weapons[1].countKills >= 3 && weapons[2].unlocked)
        {
            weapons[2].countKills = 0;
            weapons[2].AddAmmo(1);
        }
    }

    [Serializable]
    public class Weapon
    {
        public string WeaponName;
        public Sprite uiSprite;
        public WeaponBullet bullet;
        public int damage;
        public float force;
        public int bounces;
        public float lifeTime;
        public int ammo;
        public int maxAmmo;
        public bool infiniteAmmo;
        public float rateFire;
        public bool unlocked;
        public int countKills;
        public bool useReadyVfx;
        public GameObject weaponReadyVfx;

        private float _actualRate;
        private bool _canFire;
        private List<WeaponBullet> _bulletPool;
        private Transform _poolParent;

        public void InitializePool(Transform playerTransform)
        {
            _bulletPool = new List<WeaponBullet>();
            
            GameObject poolObject = new GameObject($"{WeaponName}_Pool");
            //UnityEngine.Object.DontDestroyOnLoad(poolObject);
            _poolParent = poolObject.transform;

            for (int i = 0; i < 10; i++)
            {
                CreateBulletInstance();
            }
        }


        private WeaponBullet CreateBulletInstance()
        {
            var obj = UnityEngine.Object.Instantiate(bullet.gameObject, _poolParent);
            obj.SetActive(false);
            var bulletInstance = obj.GetComponent<WeaponBullet>();
            _bulletPool.Add(bulletInstance);
            return bulletInstance;
        }


        public void Update()
        {
            if (!_canFire)
            {
                if (_actualRate > 0) _actualRate -= Time.deltaTime;
                else _canFire = true;
            }
            
            if(!useReadyVfx) return;
            
            if(ammo > 0 && unlocked && !weaponReadyVfx.activeSelf) weaponReadyVfx.SetActive(true);
            else if (ammo <= 0 && weaponReadyVfx.activeSelf) weaponReadyVfx.SetActive(false);
        }

        public void Fire(Transform spawn, int myIndex)
        {
            if (!_canFire || (!infiniteAmmo && ammo <= 0)) return;
            if (!infiniteAmmo) ammo--;

            WeaponBullet bulletToFire = null;

            foreach (var pooledBullet in _bulletPool)
            {
                if (!pooledBullet.gameObject.activeInHierarchy)
                {
                    bulletToFire = pooledBullet;
                    break;
                }
            }

            if (bulletToFire == null)
            {
                bulletToFire = CreateBulletInstance();
            }

            bulletToFire.transform.position = spawn.position;
            bulletToFire.transform.rotation = spawn.rotation;
            bulletToFire.gameObject.SetActive(true);
            bulletToFire.SetBullet(damage, force, lifeTime, bounces, myIndex);

            RateFire();
        }

        public void AddAmmo(int ammoToAdd)
        {
            ammo += ammoToAdd;
            if (ammo > maxAmmo) ammo = maxAmmo;
        }

        public void ClearKills()
        {
            countKills = 0;
        }

        private void RateFire()
        {
            _actualRate = rateFire;
            _canFire = false;
        }
    }
}
