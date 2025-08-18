using DG.Tweening;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiceRollWindow : MonoBehaviour
{
    public static DiceRollWindow Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private RectTransform windowRectTransform;
    [SerializeField] private float animationDuration = 0.5f;

    private int localPlayerId;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        localPlayerId = PhotonNetwork.LocalPlayer.ActorNumber;
        ForceHideWindow(); // сразу скрываем окно при старте
    }

    public void TurnChangeWindow(int currentPlayerId)
    {
        if (currentPlayerId == localPlayerId)
        {
            ShowWindow();
        }
        else
        {
            ForceHideWindow();
        }
    }

    /// <summary> Показ окна (только для локального игрока). </summary>
    public void ShowWindow()
    {
        windowRectTransform.DOKill(); // сброс анимаций
        windowRectTransform.DOAnchorPos(Vector2.zero, animationDuration);
    }

    /// <summary> Скрытие окна с анимацией. </summary>
    public void HideWindow()
    {
        windowRectTransform.DOKill();
        windowRectTransform.DOAnchorPos(new Vector2(0, 160), animationDuration);
    }

    /// <summary> Мгновенное скрытие (без анимации). </summary>
    private void ForceHideWindow()
    {
        windowRectTransform.anchoredPosition = new Vector2(0, 160);
    }

}
