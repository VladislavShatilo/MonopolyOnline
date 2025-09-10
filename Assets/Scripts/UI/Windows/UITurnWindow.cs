using DG.Tweening;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static System.Net.Mime.MediaTypeNames;

public class UITurnWindow : UIWindowBase,ITurnWindow
{
    [Header("UI")]
    [SerializeField] private Button throwDiceButton;

    [Header("Cheat")]
    [SerializeField] private TMP_InputField inputField;
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
    public int GetSteps()
    {
        int steps = 0;
        if (inputField.text != "")
        {
            steps = int.Parse(inputField.text);
        }
        return steps;
    }
}
