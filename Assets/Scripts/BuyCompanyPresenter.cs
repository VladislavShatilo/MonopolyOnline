using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class BuyCompanyPresenter : IBuyCompanyPresenter, IInitializable, IDisposable
{
    private ILocalPlayerService localPlayerService;
    private IPhotonCompanyManager photonCompanyManager;
    private IBuyWindow buyWindow;
    private IPhotonAuctionManager photonAuctionManager;
    private ICompanyRepository companyRepository;
    private IEventBus eventBus;

    #region LIFE_CYCLE

    [Inject]
    public void Construct(ILocalPlayerService localPlayerService, IBuyWindow buyWindow, IPhotonCompanyManager photonCompanyManager, IPhotonAuctionManager photonAuctionManager,
        ICompanyRepository companyRepository, IEventBus eventBus)
    {
        this.localPlayerService = localPlayerService ?? throw new ArgumentNullException(nameof(localPlayerService));
        this.buyWindow = buyWindow ?? throw new ArgumentNullException(nameof(buyWindow));
        this.photonCompanyManager = photonCompanyManager ?? throw new ArgumentNullException(nameof(photonCompanyManager));
        this.photonAuctionManager = photonAuctionManager ?? throw new ArgumentNullException(nameof(photonAuctionManager));
        this.companyRepository = companyRepository ?? throw new ArgumentNullException(nameof(companyRepository));
        this.eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
    }

    public void Initialize()
    {
        eventBus.Subscribe<OfferPurchaseEvent>(BuyWindowShow);

        buyWindow.SetBuyAction(TryBuyCompany);
        buyWindow.SetAuctionAction(StartAuctionRequest);
    }

    public void Dispose()
    {
        eventBus.Unsubscribe<OfferPurchaseEvent>(BuyWindowShow);
    }

    #endregion LIFE_CYCLE

    #region PUBLIC_METHODS

    public void TryBuyCompany(int cellIndex)
    {
        int localId = localPlayerService.GetLocalPlayerId();
        photonCompanyManager.RequestBuyCompany(cellIndex, localId, BuyReason.Buy);
    }

    public void StartAuctionRequest(int cellIndex)
    {
        int localId = localPlayerService.GetLocalPlayerId();
        Company company = companyRepository.GetCompanyById(cellIndex) ?? throw new InvalidOperationException(nameof(company)); 
        photonAuctionManager.StartAuctionRequest(localId, cellIndex, company.Price);
    }

    #endregion PUBLIC_METHODS

    #region CALLBAKCS

    private void BuyWindowShow(OfferPurchaseEvent e)
    {
        if (e == null)
            throw new ArgumentNullException(nameof(e));
        int localId = localPlayerService.GetLocalPlayerId();

        if (e.PlayerId == localId)
        {
            buyWindow.Show(e.PlayerId, e.CellIndex, e.Price, e.CanAfford);
        }
        else
        {
            buyWindow.Hide();
        }
    }

    #endregion CALLBAKCS
}