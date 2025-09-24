using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindowAnimation : MonoBehaviour
{
    [SerializeField] private RectTransform windowRectTransform;
    [SerializeField] private float animationDuration = 0.5f;
    [SerializeField] private float animationOffset = 160f;
    public RectTransform WindowRectTransform
    {
        get => windowRectTransform;
        set => windowRectTransform = value;
    }
    public void ShowWindow()
    {
       windowRectTransform.DOAnchorPos(Vector2.zero, animationDuration);
    }
    public void HideWindow()
    {
        windowRectTransform.DOAnchorPos(new Vector2(0, animationOffset), animationDuration);
    }
    public void HardHideWindow()
    {
        windowRectTransform.anchoredPosition = new Vector2(0, animationOffset);
    }
}
