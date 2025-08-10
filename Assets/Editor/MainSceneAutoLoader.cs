using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class MainSceneAutoLoader
{
    private const string MAIN_SCENE_PATH = "Assets\\Scenes\\ConnectToServer.unity";
    private const string PREF_KEY_PREV_SCENE = "PREVIOUS SCENE";
    [System.Obsolete]
    static MainSceneAutoLoader()
    {
        EditorApplication.playmodeStateChanged += OnPlayModeStateChanged;
    }


    private static void OnPlayModeStateChanged()
    {
        if (!EditorApplication.isPlaying && EditorApplication.isPlayingOrWillChangePlaymode)
        {
            if (SceneManager.GetActiveScene().buildIndex == 0)
            {
                return;
            }
            var path = SceneManager.GetActiveScene().path;
            EditorPrefs.SetString(PREF_KEY_PREV_SCENE, path);
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {

                try
                {
                    EditorSceneManager.OpenScene(MAIN_SCENE_PATH);
                }
                catch
                {
                    Debug.LogError($"cannot {MAIN_SCENE_PATH}");
                    EditorApplication.isPlaying = false;
                }
            }
            else
            {
                EditorApplication.isPlaying = false;
            }
        }

        if (!EditorApplication.isPlaying && !EditorApplication.isPlayingOrWillChangePlaymode)
        {
            var path = EditorPrefs.GetString(PREF_KEY_PREV_SCENE);

            try
            {
                EditorSceneManager.OpenScene(path);
            }
            catch
            {
                Debug.LogError($"Cannot load scene: {path}");
                EditorApplication.isPlaying = false;
            }
        }
    }
}

