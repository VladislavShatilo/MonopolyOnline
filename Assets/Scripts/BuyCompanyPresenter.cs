using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class BuyCompanyPresenter : IBuyCompanyPresenter, IInitializable,IDisposable
{
    private ILocalPlayerService localPlayerService;
    private IPhotonCompanyManager photonCompanyManager;
    private IBuyWindow buyWindow;
    private int cellIndex;

    [Inject]
    public void Construct(ILocalPlayerService localPlayerService, IBuyWindow buyWindow, IPhotonCompanyManager photonCompanyManager)
    {
        this.localPlayerService = localPlayerService;
        this.buyWindow = buyWindow;
        this.photonCompanyManager = photonCompanyManager;

    }
    void IInitializable.Initialize()
    {
        EventBus.Subscribe<OfferPurchaseEvent>(BuyWindowShow);
        buyWindow.SetBuyAction(TryBuyCompany);

    }

    void IDisposable.Dispose()
    {
        EventBus.Unsubscribe<OfferPurchaseEvent>(BuyWindowShow);
    }
    private void BuyWindowShow(OfferPurchaseEvent e)
    {
        int localId = localPlayerService.GetLocalPlayerId();

        if (e.PlayerId == localId)
        {
            buyWindow.Show(e.PlayerId, e.CellIndex, e.Price, e.CanAfford);
            cellIndex= e.CellIndex;

        }
        else
        {
            buyWindow.Hide();
        }
    }

    public void TryBuyCompany(int cellIndex)
    {
        int localId = localPlayerService.GetLocalPlayerId();
        photonCompanyManager.RequestBuyCompany(cellIndex, localId, BuyReason.Buy);
    }


    public void ShowTurnFor(int playerId) => HideTurn();
    public void HideTurn() => buyWindow.Hide();
}
