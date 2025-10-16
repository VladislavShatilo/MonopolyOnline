using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public enum BuyReason
{
    Buy,
    Auction
}

public class CompanyService : ICompanyService, IInitializable, IDisposable
{
    private ICompanyRepository companyRepository;
    private IBankService bank;
    private IPhotonTurnManager photonTurnManager;
    private ICompanySyncService companySyncService;
    private IPlayerRepository playerRepository;
    private IEventBus eventBus;
    private IPhotonNetworkWrapper photonNetworkWrapper;

    #region LIFE_CYCLE

    [Inject]
    public void Construct(ICompanyRepository companyRepository, IBankService bank, IPhotonTurnManager photonTurnManager, ICompanySyncService companySyncService,
       IPlayerRepository playerRepository, IEventBus eventBus, IPhotonNetworkWrapper photonNetworkWrapper)
    {
        this.companyRepository = companyRepository ?? throw new ArgumentNullException(nameof(companyRepository));
        this.bank = bank ?? throw new ArgumentNullException(nameof(bank));
        this.photonTurnManager = photonTurnManager ?? throw new ArgumentNullException(nameof(photonTurnManager));
        this.companySyncService = companySyncService ?? throw new ArgumentNullException(nameof(companySyncService));
        this.playerRepository = playerRepository ?? throw new ArgumentNullException(nameof(playerRepository));
        this.eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        this.photonNetworkWrapper = photonNetworkWrapper ?? throw new ArgumentNullException(nameof(photonNetworkWrapper));
    }

    public void Initialize()
    {
        eventBus.Subscribe<EndAuctionWithWinnerEvent>(AuctionBuyCompany);
    }

    public void Dispose()
    {
        eventBus.Unsubscribe<EndAuctionWithWinnerEvent>(AuctionBuyCompany);
    }

    #endregion LIFE_CYCLE

    #region PUBLIC_METHODS

    public void HandleCell(int cellIndex, int playerId)
    {
        var company = companyRepository.GetCompanyById(cellIndex) ?? throw new InvalidOperationException(nameof(HandleCell));

        if (!company.IsBought)
        {
            bool canAfford = bank.HasEnoughMoney(playerId, company.Price);
            eventBus.Publish(new OfferPurchaseEvent(cellIndex, playerId, company.Price, canAfford));
        }
        else if (company.OwnerId != playerId && !company.IsMortgaged)
        {
            PlayerData player = playerRepository.GetPlayerById(playerId) ?? throw new InvalidOperationException(nameof(HandleCell));
            eventBus.Publish(new OfferRentEvent(cellIndex, playerId, CalculateRent(company, player.LastDiceSum)));
        }
        else
        {
            if (photonNetworkWrapper.IsMasterClient)
            {
                photonTurnManager.RequestEndTurn();
            }
        }
    }

    public void TryBuyCompany(int cellIndex, int playerId, int price, BuyReason reason)
    {
        var company = companyRepository.GetCompanyById(cellIndex) ?? throw new InvalidOperationException(nameof(TryBuyCompany));

        if (company.IsBought) return;
        if (reason == BuyReason.Buy)
        {
            price = company.Price;
        }
        if (!bank.HasEnoughMoney(playerId, price)) return;

        company.Buy(playerId);
        bank.RemoveMoney(playerId, price);
        eventBus.Publish(new CompanyBoughtEvent(cellIndex, playerId));
        photonTurnManager.RequestEndTurn();
        companySyncService.SyncCompanyBought(cellIndex, playerId, price);
    }

    public void TryPayRent(int cellIndex, int playerId)
    {
        var company = companyRepository.GetCompanyById(cellIndex) ?? throw new InvalidOperationException(nameof(TryPayRent));
        if (!company.IsBought) return;
        PlayerData player = playerRepository.GetPlayerById(playerId) ?? throw new InvalidOperationException(nameof(TryPayRent));

        int rent = CalculateRent(company, player.LastDiceSum);
        if (!bank.HasEnoughMoney(playerId, rent)) return;

        bank.TransferMoney(playerId, company.OwnerId, rent);
        eventBus.Publish(new RentPaidEvent(cellIndex, playerId, company.OwnerId, rent));
        photonTurnManager.RequestEndTurn();

        companySyncService.SyncRentPaid(cellIndex, playerId, company.OwnerId, rent);
    }

    public int CalculateRent(Company company, int diceSum)
    {
        var ownedCount = companyRepository.CountOwnedByPlayer(company.OwnerId, company.Type);
        return company.GetRent(ownedCount, diceSum);
    }

    #endregion PUBLIC_METHODS

    #region PRIVATE_METHODS

    private void AuctionBuyCompany(EndAuctionWithWinnerEvent e)
    {
        TryBuyCompany(e.CompanyId, e.WinnerId, e.FinalPrice, BuyReason.Auction);
    }

    #endregion PRIVATE_METHODS
}