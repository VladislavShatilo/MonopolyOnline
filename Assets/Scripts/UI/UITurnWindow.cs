using DG.Tweening;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UITurnWindow : MonoBehaviour
{
    public static UITurnWindow Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private RectTransform windowRectTransform;
    [SerializeField] private float animationDuration = 0.5f;
    [SerializeField] private Button throwDiceButton;
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
    public  void OnEnable()
    {

        EventBus.Subscribe<TurnStartEvent>(TurnChangeWindow);
        throwDiceButton.onClick.AddListener(OnThrowButtonClick);

    }

    public  void OnDisable()
    {
        EventBus.Unsubscribe<TurnStartEvent>(TurnChangeWindow);
        throwDiceButton.onClick.RemoveListener(OnThrowButtonClick);

    }
    private void Start()
    {
        localPlayerId = PhotonNetwork.LocalPlayer.ActorNumber;
        ForceHideWindow(); // сразу скрываем окно при старте
    }
    private void OnThrowButtonClick()
    {
        EventBus.Publish(new RollDiceButtonEvent(localPlayerId));
    }
    public void TurnChangeWindow(TurnStartEvent e)
    {
        if (e.PlayerId == localPlayerId)
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
