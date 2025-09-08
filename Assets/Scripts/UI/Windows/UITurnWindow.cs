using DG.Tweening;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UITurnWindow : UIWindowBase,ITurnWindow
{
    [Header("UI")]
    [SerializeField] private Button throwDiceButton;

    public Button ThrowDiceButton
    {
        get => throwDiceButton;
        set => throwDiceButton = value;
    }

    public void Show() => ShowWindow();
    public void Hide() => HideWindow();
    public void HardHide() => HardHideWindow();
    public void SetThrowDiceAction(System.Action onClick)
    {
        throwDiceButton.onClick.RemoveAllListeners();
        if (onClick != null)
        {
            throwDiceButton.onClick.AddListener(() => onClick());
        }
    }
}
