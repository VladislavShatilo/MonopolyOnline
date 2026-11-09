using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeService : MonoBehaviour, IFadeService 
{ 
    [SerializeField] private Animator animator;

    private Action fadeInCallback;
    private Action fadeOutCallback;
    public bool IsFading { get; private set; }

    public void FadeIn(Action callback)
    {
        if (IsFading) return;
        IsFading = true;
        fadeInCallback = callback;
        animator.SetBool("faded", true);
    }

    public void FadeOut(Action callback)
    {
        if (IsFading) return;
        IsFading = true;
        fadeOutCallback = callback;
        animator.SetBool("faded", false);
    }

    // Эти методы вызываются через Animation Event
    public void OnFadeInFinished()
    {
        fadeInCallback?.Invoke();
        fadeInCallback = null;
        IsFading = false;
    }

    public void OnFadeOutFinished()
    {
        fadeOutCallback?.Invoke();
        fadeOutCallback = null;
        IsFading = false;
    }
}
