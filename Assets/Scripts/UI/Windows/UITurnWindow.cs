using DG.Tweening;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UITurnWindow : UIWindowBase<UITurnWindow>
{

    [Header("UI")]
    [SerializeField] private Button throwDiceButton;
    private int localPlayerId;
    public Button ThrowDiceButton
    {
        get => throwDiceButton;
        set => throwDiceButton = value;
    }
    protected override void SubscribeEvents()
    {
        EventBus.Subscribe<TurnStartEvent>(TurnChangeWindow);
    }

    protected override void UnsubscribeEvents()
    {
        EventBus.Unsubscribe<TurnStartEvent>(TurnChangeWindow);
    }
    protected override void OnEnable()
    {
        base.OnEnable();
        if(throwDiceButton != null)
        {
            throwDiceButton.onClick.AddListener(OnThrowButtonClick);
        }

    }

    protected override void OnDisable()
    {
        base.OnDisable();
        if (throwDiceButton != null)
        {
            throwDiceButton.onClick.RemoveListener(OnThrowButtonClick);
        }

    }
    private void Start()
    {
        localPlayerId = PhotonNetwork.LocalPlayer.ActorNumber;
    }
    private void OnThrowButtonClick()
    {
        EventBus.Publish(new RollDiceButtonEvent(localPlayerId)); 
        HideWindow();

    }
    public void TurnChangeWindow(TurnStartEvent e)
    {
        if (e.PlayerId == localPlayerId)
        {
            ShowWindow();
        }
        else
        {
            HardHideWindow();
        }
        
    }
    

}
