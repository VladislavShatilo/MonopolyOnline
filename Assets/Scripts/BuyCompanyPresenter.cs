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
    private IPhotonAuctionManager photonAuctionManager;
    private ICompanyRepository companyRepository;
    private IEventBus eventBus;
    private int cellIndex;

    [Inject]
    public void Construct(ILocalPlayerService localPlayerService, IBuyWindow buyWindow, IPhotonCompanyManager photonCompanyManager, IPhotonAuctionManager photonAuctionManager,
        ICompanyRepository companyRepository, IEventBus eventBus)
    {
        this.localPlayerService = localPlayerService;
        this.buyWindow = buyWindow;
        this.photonCompanyManager = photonCompanyManager;
        this.photonAuctionManager = photonAuctionManager;
        this.companyRepository = companyRepository;
        this.eventBus = eventBus;

    }
    void IInitializable.Initialize()
    {
        eventBus.Subscribe<OfferPurchaseEvent>(BuyWindowShow);
        buyWindow.SetBuyAction(TryBuyCompany);
        buyWindow.SetAuctionAction(StartAuctionRequest);

    }

    void IDisposable.Dispose()
    {
        eventBus.Unsubscribe<OfferPurchaseEvent>(BuyWindowShow);
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
    public void StartAuctionRequest(int cellIndex)
    {

        int localId = localPlayerService.GetLocalPlayerId();
        Company company = companyRepository.GetCompanyById(cellIndex);
        photonAuctionManager.StartAuctionRequest(localId, cellIndex,company.Price);
    }

    public void ShowTurnFor(int playerId) => HideTurn();
    public void HideTurn() => buyWindow.Hide();
}
