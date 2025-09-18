using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class IntroVideoPlay : MonoBehaviour
{
    [SerializeField] string sceneOpen = "MainMenu";
    [SerializeField] VideoClip[] clips;
    [SerializeField] CanvasGroup screensplash;
    [SerializeField] bool isTest = false;


    VideoPlayer videoPlayer;
    bool isSceneLoad = false;
    AsyncOperation asyncOperation;
    bool isLoadSaveGame = false;    
    int curClip = 0;

    public bool IsLoadSaveGame { get => isLoadSaveGame; set => isLoadSaveGame = value; }

    private void OnEnable()
    {
        GDKGame.OnGameSaveLoaded += OnGameSaveLoaded;
    }
    private void OnDisable()
    {
        GDKGame.OnGameSaveLoaded -= OnGameSaveLoaded;
    }

    private void OnGameSaveLoaded(object sender, GDKGame.GameSaveLoadedArgs e)
    {

        Debug.Log("OnGameSaveLoaded");
        isLoadSaveGame = true;
    }

    IEnumerator Start()
    {
        StartCoroutine(LoadSceneAsyncCoroutine());

        videoPlayer = Camera.main.GetComponent<VideoPlayer>();
        videoPlayer.clip = clips[0];
        // Skip the first 100 frames.
        videoPlayer.frame = 0;

        // Each time we reach the end, we slow down the playback by a factor of 10.
        videoPlayer.loopPointReached += EndReached;

        while (GDKUserInfo.userHandle == null)        
            yield return null;        

        while (!isLoadSaveGame)
            yield return null;

        StartCoroutine(FadeOut(screensplash, 1f));
        while(screensplash.alpha > 0)
            yield return null;


        if (isTest)
            StartCoroutine(NextScene());
        else
            videoPlayer.Play();            
    }

    void EndReached(VideoPlayer vp)
    {
        Debug.Log("EndReached");
        curClip++;
        if (curClip < clips.Length)
        {
            videoPlayer.clip = clips[curClip];
            videoPlayer.Play();
        }
        else
        {
            StartCoroutine(NextScene());
        }
    }

    IEnumerator NextScene()
    {
        while(GDKUserInfo.userHandle == null)
        {
            yield return null;
        }

        //Debug.Log("NextLevel");
        while (videoPlayer.isPlaying || !isSceneLoad || !isLoadSaveGame)
        {
            Debug.Log($"{videoPlayer.isPlaying} || {!isSceneLoad} || {!isLoadSaveGame}");
            yield return null;
        }

        yield return new WaitForSeconds(1f);
        asyncOperation.allowSceneActivation = true;
        enabled = false;
    }

    IEnumerator LoadSceneAsyncCoroutine()
    {
        yield return new WaitForSeconds(0.5f);

        asyncOperation = SceneManager.LoadSceneAsync(sceneOpen);

        asyncOperation.allowSceneActivation = false;

        while (!asyncOperation.isDone)
        {
            float progress = Mathf.Clamp01(asyncOperation.progress / 0.9f); 

            if (progress >= 0.9f)
            {
                // Aguarda um pequeno intervalo para manter a animação visível
                yield return new WaitForSeconds(1f);

                isSceneLoad = true;
                yield break;
            }

            yield return null;
        }
    }
    private IEnumerator FadeOut(CanvasGroup canvasGroup, float duration)
    {
        float startAlpha = canvasGroup.alpha;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, elapsedTime / duration);
            yield return null;
        }

        canvasGroup.alpha = 0f;
    }
}
