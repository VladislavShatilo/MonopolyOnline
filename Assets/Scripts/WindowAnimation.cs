using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindowAnimation : MonoBehaviour
{
    [SerializeField] private RectTransform windowRectTransform;
    [SerializeField] private float animationDuration = 0.5f;
    [SerializeField] private float animationOffset = 160f;

    #region PUBLIC_METHODS
    private void Awake()
    {
        if(windowRectTransform == null)
        {
            throw new NullReferenceException(nameof(WindowAnimation));
        }
    }
    public RectTransform WindowRectTransform
    {
        get => windowRectTransform;
        set => windowRectTransform = value;
    }
    public virtual void ShowWindow()
    {
        if(windowRectTransform != null)
        {
            windowRectTransform.DOAnchorPos(Vector2.zero, animationDuration);
        }
    }
    public virtual void HideWindow()
    {
        if (windowRectTransform != null)
        {
            windowRectTransform.DOAnchorPos(new Vector2(0, animationOffset), animationDuration);
        }
    }
    public virtual void HardHideWindow()
    {
        if (windowRectTransform != null)
        {
            windowRectTransform.anchoredPosition = new Vector2(0, animationOffset);
        }
    }

    #endregion PUBLIC_METHODS

}
