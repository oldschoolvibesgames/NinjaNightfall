using UnityEngine;

#if MICROSOFT_GDK_SUPPORT || UNITY_GAMECORE
using Unity.XGamingRuntime;
#endif

public class GDKAchievements : MonoBehaviour
{
#if MICROSOFT_GDK_SUPPORT || UNITY_GAMECORE
    public static GDKAchievements Instance { get; private set; }

    private const int _100PercentAchievementProgress = 100;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.Log("Singleton trying to create another instance");
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(Instance);
        }
    }

    public void UnlockAchievement(string achievementId)
    {
        ulong xuid;

        int hResult = SDK.XUserGetId(GDKUserInfo.userHandle, out xuid);
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
            GDKUserInfo.xblContextHandle, xuid, achievementId,
            _100PercentAchievementProgress, UnlockAchievementComplete
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
            Debug.LogError($"FAILED: Achievement Update, hResult=0x{hResult:X} ({HR.NameOf(hResult)})");
            return;
        }

        Debug.Log($"SUCCESS: {message}");
    }
#endif
}