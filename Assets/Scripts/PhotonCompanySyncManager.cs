using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class PhotonCompanySyncManager : MonoBehaviourPun, ICompanySyncService
{
    private ICompanyRepository companyRepository;
    private IPlayerRepository playerRepository;
    private IEventBus eventBus;

    #region LIFE_CYCLE

    [Inject]
    public void Construct(ICompanyRepository companyRepository, IPlayerRepository playerRepository, IEventBus eventBus)
    {
        this.companyRepository = companyRepository;
        this.playerRepository = playerRepository;
        this.eventBus = eventBus;
    }

    #endregion LIFE_CYCLE

    #region PUBLIC_METHODS

    public void SyncCompanyBought(int companyId, int playerId, int price)
    {

        photonView.RPC(nameof(RPC_SyncCompanyBought), RpcTarget.Others, companyId, playerId, price);
    }

    public void SyncRentPaid(int companyId, int playerId, int ownerId, int rent)
    {
        photonView.RPC(nameof(RPC_SyncRentPaid), RpcTarget.Others, companyId, playerId, ownerId, rent);
    }

    #endregion PUBLIC_METHODS

    #region RPC

    [PunRPC]
    private void RPC_SyncCompanyBought(int companyId, int playerId, int price)
    {
        var company = companyRepository.GetCompanyById(companyId);
        company.Buy(playerId);

        var player = playerRepository.GetPlayerById(playerId);
        player.Money -= price; // синхронизация, не логика банка
        eventBus.Publish(new CompanyBoughtEvent(companyId, playerId));
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

    #endregion RPC

}
