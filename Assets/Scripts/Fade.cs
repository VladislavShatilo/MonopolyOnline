using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fade : MonoBehaviour
{
    private const string FADE_PATH = "FadeCanvas";
    private static Fade instance;
    [SerializeField] private Animator animator;
    public static Fade Instance
    {
        get
        {
            if (instance == null)
            {
                var prefab = Resources.Load<Fade>(FADE_PATH);
                instance = Instantiate(prefab);
                DontDestroyOnLoad(instance.gameObject);
            }
            return instance;
        }
    }
    public bool isFading { get; private set; }
    private Action fadedInCallback;
    private Action fadedOutCallback;

    public void FadeIn(Action fadedInCallback)
    {
        if (isFading)
        {
            return;
        }
        isFading = true;
        this.fadedInCallback = fadedInCallback;
        animator.SetBool("faded", true);

    }
    public void FadeOut(Action fadedOutCallback)
    {
        if (isFading)
        {
            return;
        }
        isFading = true;
        this.fadedOutCallback = fadedOutCallback;
        animator.SetBool("faded", false);
    }

    private void HandleFadeInAnimationOver()
    {
        fadedInCallback?.Invoke();
        fadedInCallback = null;
        isFading = false;
    }
    private void HandleFadeOutAnimationOver()
    {
        fadedOutCallback?.Invoke();
        fadedOutCallback = null;
        isFading = false;
    }
}
