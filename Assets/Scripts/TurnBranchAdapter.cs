using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class TurnBranchAdapter : MonoBehaviour
{
    private IBranchTurnHandler branchTurnHandler; 
    private IEventBus eventBus;


    [Inject]
    public void Construct(IBranchTurnHandler branchTurnHandler, IEventBus eventBus)
    {
        this.branchTurnHandler = branchTurnHandler;
        this.eventBus = eventBus;
    }

    private void OnEnable()
    {
        eventBus.Subscribe<TurnStartEvent>(HandleTurnStart);
    }

    private void OnDisable()
    {
        eventBus.Unsubscribe<TurnStartEvent>(HandleTurnStart);
    }

    private void HandleTurnStart(TurnStartEvent e)
    {
        branchTurnHandler.OnTurnStart(e.PlayerId, PhotonNetwork.LocalPlayer.ActorNumber);
    }
}
