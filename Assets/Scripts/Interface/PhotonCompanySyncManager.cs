using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class PhotonCompanySyncManager : MonoBehaviourPun, ICompanySyncService
{
    private ICompanyRepository companyRepository;
    private IPlayerRepository playerRepository;
    private IBankService bankService;
    private IEventBus eventBus;
    [Inject]
    public void Construct( ICompanyRepository companyRepository, IPlayerRepository playerRepository,IBankService bankService, IEventBus eventBus)
    {
        this.companyRepository = companyRepository;
        this.playerRepository = playerRepository;
        this.bankService = bankService;
        this.eventBus = eventBus;
    }
    [PunRPC]
    private void RPC_SyncCompanyBought(int companyId, int playerId, int price, int reason)
    {
        var company = companyRepository.GetCompanyById(companyId);
        company.Buy(playerId);

        var player = playerRepository.GetPlayerById(playerId);
        player.Money -= price; // синхронизация, не логика банка
        eventBus.Publish(new CompanyBoughtEvent(companyId, playerId, price, (BuyReason)reason));
    }
    [PunRPC]
    private void RPC_SyncRentPaid(int companyId, int playerId, int ownerId, int rent)
    {
        var company = companyRepository.GetCompanyById(companyId);
        var owner = playerRepository.GetPlayerById(ownerId);
        var renter = playerRepository.GetPlayerById(playerId);

        owner.Money += rent;
        renter.Money -= rent;

        eventBus.Publish(new RentPaidEvent(companyId, playerId, company.OwnerId, rent));

      

    }
    public void SyncCompanyBought(int companyId, int playerId, int price, BuyReason reason)
    {

        photonView.RPC(nameof(RPC_SyncCompanyBought), RpcTarget.Others, companyId, playerId, price, (int)reason);
    }

    public void SyncRentPaid(int companyId, int playerId, int ownerId, int rent)
    {
        photonView.RPC(nameof(RPC_SyncRentPaid), RpcTarget.Others, companyId, playerId, ownerId, rent);
    }
}
