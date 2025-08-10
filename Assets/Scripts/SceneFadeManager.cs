using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneFadeManager : MonoBehaviour
{
    private const string SCENE_AUTORIZTION = "AutorizationScene";
    private const string SCENE_LOBBY = "LobbyScene";
    private const string SCENE_GAME = "GameScene";

    private bool isLoading;

    public static SceneFadeManager instance;
    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void LoadAutorizationScene()
    {
        LoadScene(SCENE_AUTORIZTION);
    }
    public void LoadLobbyScene()
    {
        LoadScene(SCENE_LOBBY);
    }
    public void LoadGameScene()
    {
        LoadScene(SCENE_GAME);
    }



    private void LoadScene(string sceneName)
    {
        if (isLoading) return;

        var currentSceneName = SceneManager.GetActiveScene().name;

        StartCoroutine(LoadSceneRoutine(sceneName));

    }

    private IEnumerator LoadSceneRoutine(string sceneName)
    {
        isLoading = true;

        var waitFading = true;
        Fade.Instance.FadeIn(() => waitFading = false);

        while (waitFading)
        {
            yield return null;
        }
        var async = SceneManager.LoadSceneAsync(sceneName);
        async.allowSceneActivation = false;

        while (async.progress < 0.9f)
        {
            yield return null;
        }
        async.allowSceneActivation = true;

        waitFading = true;
        Fade.Instance.FadeOut(() => waitFading = false);

        while (waitFading)
        {
            yield return null;
        }
        isLoading = false;
    }
}
