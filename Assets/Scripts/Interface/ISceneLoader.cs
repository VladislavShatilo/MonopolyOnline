using System;
using System.Collections;
public interface ISceneLoader
{
    void LoadScene(string sceneName);
    IEnumerator LoadSceneRoutine(string sceneName);
}
