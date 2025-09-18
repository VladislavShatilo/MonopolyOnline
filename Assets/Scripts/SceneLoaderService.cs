using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

public class SceneLoaderService : MonoBehaviour, ISceneLoader
{
    private IFadeService fadeService;
    private bool isLoading = false;

    [Inject]
    public void Construct(IFadeService fadeService)
    {
        this.fadeService = fadeService;
    }

    public void LoadScene(string sceneName)
    {
        if (isLoading) return;
        StartCoroutine(LoadSceneRoutine(sceneName));
    }

    public IEnumerator LoadSceneRoutine(string sceneName)
    {
        isLoading = true;

        bool waitFading = true;
        fadeService.FadeIn(() => waitFading = false);
        while (waitFading) yield return null;

        var async = SceneManager.LoadSceneAsync(sceneName);
        async.allowSceneActivation = false;

        while (async.progress < 0.9f) yield return null;

        async.allowSceneActivation = true;

        waitFading = true;
        fadeService.FadeOut(() => waitFading = false);
        while (waitFading) yield return null;

        isLoading = false;
    }
}
