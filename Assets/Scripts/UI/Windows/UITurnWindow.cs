using DG.Tweening;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UITurnWindow : UIWindowBase,ITurnWindow
{
    [Header("UI")]
    [SerializeField] private Button throwDiceButton;

    [Header("Cheat")]
    [SerializeField] private TMP_InputField inputField1;
    [SerializeField] private TMP_InputField inputField2;

    #region PUBLIC_METHODS

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
    public int GetSteps1()
    {
        int steps = 0;
        if (inputField1.text != "")
        {
            steps = int.Parse(inputField1.text);
        }
        return steps;
    }
    public int GetSteps2()
    {
        int steps = 0;
        if (inputField2.text != "")
        {
            steps = int.Parse(inputField2.text);
        }
        return steps;
    }
    public Button ThrowDiceButtonPublic
    {
        get => throwDiceButton;
        set => throwDiceButton = value;
    }

    public TMP_InputField InputField1Public
    {
        get => inputField1;
        set => inputField1 = value;
    }

    public TMP_InputField InputField2Public
    {
        get => inputField2;
        set => inputField2 = value;
    }
    #endregion PUBLIC_METHODS


}
