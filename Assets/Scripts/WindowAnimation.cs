using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindowAnimation : MonoBehaviour
{
    [SerializeField] private RectTransform windowRectTransform;
    [SerializeField] private float animationDuration = 0.5f;

    public void ShowWindow()
    {
        windowRectTransform.DOAnchorPos(Vector2.zero, animationDuration);
    }
    public void HideWindow()
    {
        windowRectTransform.DOAnchorPos(new Vector2(0, 160), animationDuration);
    }

}
