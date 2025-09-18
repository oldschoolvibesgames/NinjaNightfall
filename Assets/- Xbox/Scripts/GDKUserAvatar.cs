using UnityEngine;
using UnityEngine.UI;

#if MICROSOFT_GDK_SUPPORT || UNITY_GAMECORE
using Unity.XGamingRuntime;
#endif

public class GDKUserAvatar : MonoBehaviour
{
    [SerializeField] private GameObject loader;

#if MICROSOFT_GDK_SUPPORT || UNITY_GAMECORE
    private Image avatarImage;

    void Start()
    {
        avatarImage = GetComponent<Image>();
        LoadAvatarImage();
    }

    public void LoadAvatarImage()
    {
        //SDK.XUserGetGamerPictureAsync(GDKUserInfo.userHandle,
        //    XUserGamerPictureSize.Medium, OnPictureLoaded);

        // Check GDK initialization
        if (!GDKGameRuntime.Initialized)
        {
            Debug.LogWarning("GDK not initialized - cannot load avatar");
            return;
        }

        // Check user handle
        if (GDKUserInfo.userHandle == null || GDKUserInfo.userHandle.IsInvalid)
        {
            Debug.LogWarning("Invalid user handle - cannot load avatar");
            return;
        }
        
        loader.SetActive(true);

        try
        {
            SDK.XUserGetGamerPictureAsync(
                GDKUserInfo.userHandle,
                XUserGamerPictureSize.Medium,
                OnPictureLoaded);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error loading avatar: {e.Message}");            
            loader.SetActive(false);
        }
    }

    private void OnPictureLoaded(int hResult, byte[] buffer)
    {
        if (HR.FAILED(hResult))
        {
            Debug.LogWarning($"FAILED: Could not get avatar picture, hResult={hResult:X} ({HR.NameOf(hResult)})");
            return;
        }

        avatarImage.sprite = GetByteArrayAsSprite(buffer);
        loader.SetActive(false);
    }

    Sprite GetByteArrayAsSprite(byte[] imageBytes)
    {
        if (imageBytes == null || imageBytes.Length == 0) return null;

        Texture2D imageAsTexture = new Texture2D(208, 208, TextureFormat.RGBA32, false, true);
        imageAsTexture.LoadImage(imageBytes);
        imageAsTexture.Apply();

        return Sprite.Create(imageAsTexture, new Rect(0, 0, imageAsTexture.width, imageAsTexture.height), Vector2.zero);
    }
#endif
}