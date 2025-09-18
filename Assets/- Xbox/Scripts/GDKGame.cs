using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Unity.XGamingRuntime;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GDKGame : MonoBehaviour
{
    public static GDKGame instance;    

    [Header("Link")]
    [SerializeField] private IntroVideoPlay intro;

    private XUserHandle _userHandle;
    private XblContextHandle _xblContextHandle;
    private XGameSaveWrapper _gameSaveHelper;
    private XUserChangeRegistrationToken _registrationToken;
    private bool _xGameSaveInitialized;

    private const string _GameSaveContainerName = "emerald_default_container";
    private const string _GameSaveBlobName = "emerald_default_blob";
    private const int _100PercentAchievementProgress = 100;
    //private string _helperText;

    public SaveData playerSaveData;

    //public SaveDatabase playerSaveData;

    public class GameSaveLoadedArgs : System.EventArgs
    {
        public byte[] Data { get; private set; }

        public GameSaveLoadedArgs(byte[] data)
        {
            this.Data = data;
        }
    }

    public delegate void OnGameSaveLoadedHandler(object sender, GameSaveLoadedArgs e);

#pragma warning disable 0067 // Called when MICROSOFT_GAME_CORE is defined

    public static event OnGameSaveLoadedHandler OnGameSaveLoaded;

#pragma warning restore 0067

    // Start is called before the first frame update
    private void Start()
    {
        if (instance != null)
            Destroy(instance.gameObject);

        instance = this;
        DontDestroyOnLoad(this.gameObject);
        StartGame();        

    }

    public void StartGame()
    {
        playerSaveData = new SaveData();

        // Do initialization
        if (GDKGameRuntime.TryInitialize())
        {
            Debug.Log("Executou Start Game");

            InitializeAndAddUser();
            SDK.XUserRegisterForChangeEvent(UserChangeEventCallback, out _registrationToken);

            _gameSaveHelper = new XGameSaveWrapper();
            OnGameSaveLoaded += OnGameSaveLoadedCallback;
        }
        
    }

    private void OnDestroy()
    {
        if (_xblContextHandle != null)
        {
            SDK.XBL.XblContextCloseHandle(_xblContextHandle);
            _xblContextHandle = null;
        }

        if (_userHandle != null)
        {
            SDK.XUserCloseHandle(_userHandle);
            _userHandle = null;
        }

        Debug.Log(_registrationToken);

        if(_registrationToken != null)
            SDK.XUserUnregisterForChangeEvent(_registrationToken);
    }

    private void InitializeAndAddUser()
    {
        SDK.XUserAddAsync(XUserAddOptions.AddDefaultUserAllowingUI, AddUserComplete);
    }

    private void AddUserComplete(int hResult, XUserHandle userHandle)
    {
        if (HR.FAILED(hResult))
        {
            Debug.LogWarning($"FAILED: Could not signin a user, hResult={hResult:X} ({HR.NameOf(hResult)})");
            return;
        }

        _userHandle = userHandle;
        GDKUserInfo.userHandle = _userHandle;

        CompletePostSignInInitialization();
    }

    private void UserChangeEventCallback(IntPtr _, XUserLocalId userLocalId, XUserChangeEvent eventType)
    {
        if (eventType == XUserChangeEvent.SignedOut)
        {
            Debug.LogWarning("User logging out");           

            if (_xblContextHandle != null)
            {
                SDK.XBL.XblContextCloseHandle(_xblContextHandle);
                _xblContextHandle = null;
            }

            if (_userHandle != null)
            {
                SDK.XUserCloseHandle(_userHandle);
                _userHandle = null;
            }

            //if (GDKGameRuntime.Initialized)
            //{
            //    InitializeAndAddUser();
            //}

            OnGameSaveLoaded -= OnGameSaveLoadedCallback;            

            Destroy(GameObject.Find("GDKGamingRuntimeManager").gameObject);
            GDKGameRuntime.Initialized = false;
            
            SceneManager.LoadScene(0);           
        }
    }

    private void CompletePostSignInInitialization()
    {
        string gamertag = string.Empty;

        int hResult = SDK.XUserGetGamertag(_userHandle, XUserGamertagComponent.UniqueModern, out gamertag);
        if (HR.FAILED(hResult))
        {
            Debug.LogWarning($"FAILED: Could not get user tag, hResult=0x{hResult:X} ({HR.NameOf(hResult)})");
            return;
        }       

        hResult = SDK.XBL.XblContextCreateHandle(_userHandle, out _xblContextHandle);
        if (HR.FAILED(hResult))
        {
            Debug.LogError($"FAILED: Could not create context handle, hResult=0x{hResult:X} ({HR.NameOf(hResult)})");
        }

        ulong xuid;
        hResult = SDK.XUserGetId(_userHandle, out xuid);
        if (HR.FAILED(hResult))
        {
            Debug.LogError($"FAILED: Could not get user XUID, hResult=0x{hResult:X} ({HR.NameOf(hResult)})");            
            return;
        }
        
        Debug.Log($"SUCCESS: XUserGetId() returned: '{xuid}'");

        _gameSaveHelper.InitializeAsync(
            _userHandle,
            GDKGameRuntime.GameConfigScid,
            XGameSaveInitializeCompleted);

        GDKUserInfo.gamertag = gamertag;
        GDKUserInfo.xblContextHandle = _xblContextHandle;
        GDKUserInfo.xuid = xuid;

        Debug.Log("User handle: " + GDKUserInfo.userHandle);
        Debug.Log("Gamertag: " + GDKUserInfo.gamertag);
        Debug.Log("Context handle: " + GDKUserInfo.xblContextHandle);
        Debug.Log("XUID: " + GDKUserInfo.xuid);
    }

    public void SaveGame()
    {
        if (_userHandle == null || _xGameSaveInitialized == false)
        {
            return;
        }

        BinaryFormatter binaryFormatter = new BinaryFormatter();

        using (MemoryStream memoryStream = new MemoryStream())
        {
            binaryFormatter.Serialize(memoryStream, playerSaveData);
            SaveData(memoryStream.ToArray());            
        }
    }

    private void OnGameSaveLoadedCallback(object sender, GameSaveLoadedArgs saveData)
    {
        BinaryFormatter binaryFormatter = new BinaryFormatter();
        using (MemoryStream memoryStream = new MemoryStream(saveData.Data))
        {
            object playerSaveDataObj = binaryFormatter.Deserialize(memoryStream);           

            if (playerSaveDataObj is SaveData)
            {
                Debug.Log("Carregou nuvem");
                this.playerSaveData = playerSaveDataObj as SaveData;
            }
            else
            {
                Debug.Log("Criou novos dados");
                this.playerSaveData = new SaveData();
            }
        }
    }

    private void XGameSaveInitializeCompleted(int hResult)
    {
        if (HR.FAILED(hResult))
        {
            Debug.LogError($"FAILED: Initialize game save provider");
            return;
        }

        _xGameSaveInitialized = true;
        LoadSaveGame();
    }

    public void SaveData(byte[] data)
    {
        _gameSaveHelper.Save(
            _GameSaveContainerName,
            _GameSaveBlobName,
            data,
            GameSaveSaveCompleted);
    }

    private void GameSaveSaveCompleted(int hResult)
    {
        if (HR.FAILED(hResult))
        {
            if (hResult == HR.E_GS_USER_NOT_REGISTERED_IN_SERVICE)
            {
                Debug.LogError($"FAILED: User may be logging out x{hResult:X} ({HR.NameOf(hResult)})");
            }
            else
            {
                Debug.LogError($"FAILED: Game save submit, hResult=0x{hResult:X} ({HR.NameOf(hResult)})");
            }
            return;
        }

        //Debug.Log($"SUCCESS: Game save submit update complete");
    }

    public void LoadSaveGame()
    {
        if (_userHandle == null || _xGameSaveInitialized == false)
        {
            return;
        }

        _gameSaveHelper.Load(
            _GameSaveContainerName,
            _GameSaveBlobName,
            GameSaveLoadCompleted);
    }

    private void GameSaveLoadCompleted(int hResult, byte[] savedData)
    {
        if (HR.FAILED(hResult))
        {
            if (hResult == HR.E_GS_USER_NOT_REGISTERED_IN_SERVICE)
            {
                Debug.LogError($"FAILED: User may be logging out x{hResult:X} ({HR.NameOf(hResult)})");
            }

            //Debug.LogError($"FAILED: Game load, hResult=0x{hResult:X} ({HR.NameOf(hResult)})");
            //playerSaveData = new SaveData();
            playerSaveData = new SaveData();
            SaveGame();
            //Try Load Game Again
            LoadSaveGame();
            return;

        }

        if (OnGameSaveLoaded != null)
        {
            OnGameSaveLoaded(this, new GameSaveLoadedArgs(savedData));
        }

        //Debug.Log($"SUCCESS: Game save load complete");
    }

    public void UnlockAchievement(string achievementId)
    {
        Debug.Log("Unlock Achievement Test: " + achievementId);

        ulong xuid;

        int hResult = SDK.XUserGetId(_userHandle, out xuid);
        if (HR.FAILED(hResult))
        {
            Debug.LogError($"FAILED: Could not get user ID, hResult=0x{hResult:X} ({HR.NameOf(hResult)})");
            return;
        }

        // This API will work even when offline.  Offline updates will be posted by the system when connection is
        // re-established even if the title isn’t running. If the achievement has already been unlocked or the progress
        // value is less than or equal to what is currently recorded on the server HTTP_E_STATUS_NOT_MODIFIED (0x80190130L)
        // will be returned.
        SDK.XBL.XblAchievementsUpdateAchievementAsync(
            _xblContextHandle,
            xuid,
            achievementId,
            _100PercentAchievementProgress,
            UnlockAchievementComplete
        );
    }

    private void UnlockAchievementComplete(int hResult)
    {
        string message = "Achievement Unlocked!";

        if (hResult == HR.HTTP_E_STATUS_NOT_MODIFIED)
        {
            message = "Achievement ALREADY Unlocked!";
        }
        else if (HR.FAILED(hResult))
        {
            Debug.Log($"FAILED: Achievement Update, hResult=0x{hResult:X} ({HR.NameOf(hResult)})");
            return;
        }

        Debug.Log($"SUCCESS: {message}");
    }

    private void OnApplicationQuit()
    {
        SaveGame();
    }
}
