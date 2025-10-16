using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Zenject;

public class PhotonCompanySyncManager : MonoBehaviourPun, ICompanySyncService
{
    private ICompanyRepository companyRepository;
    private IPhotonViewWrapper photonViewWrapper;
    private IPlayerRepository playerRepository;
    private IEventBus eventBus;

    #region LIFE_CYCLE

    [Inject]
    public void Construct(ICompanyRepository companyRepository, IPlayerRepository playerRepository, IEventBus eventBus, IPhotonViewWrapper photonViewWrapper)
    {
        this.companyRepository = companyRepository ?? throw new ArgumentNullException(nameof(companyRepository));
        this.playerRepository = playerRepository ?? throw new ArgumentNullException(nameof(playerRepository));
        this.eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        this.photonViewWrapper = photonViewWrapper ?? throw new ArgumentNullException(nameof(photonViewWrapper));
        if (photonView == null) throw new NullReferenceException(nameof(photonView));

    }

    #endregion LIFE_CYCLE

    #region PUBLIC_METHODS

    public void SyncCompanyBought(int companyId, int playerId, int price)
    {
        photonViewWrapper.RPC(photonView, nameof(RPC_SyncCompanyBought), RpcTarget.Others, companyId, playerId, price);
    }

    public void SyncRentPaid(int companyId, int playerId, int ownerId, int rent)
    {
        photonViewWrapper.RPC(photonView, nameof(RPC_SyncRentPaid), RpcTarget.Others, companyId, playerId, ownerId, rent);
    }

    #endregion PUBLIC_METHODS

    #region RPC

    [PunRPC]
    private void RPC_SyncCompanyBought(int companyId, int playerId, int price)
    {
        var company = companyRepository.GetCompanyById(companyId) ?? throw new NullReferenceException(nameof(RPC_SyncCompanyBought)); 
        company.Buy(playerId);

        var player = playerRepository.GetPlayerById(playerId) ?? throw new NullReferenceException(nameof(RPC_SyncCompanyBought)); 
        player.Money -= price; 

        eventBus.Publish(new CompanyBoughtEvent(companyId, playerId));
    }
    [PunRPC]
    private void RPC_SyncRentPaid(int companyId, int playerId, int ownerId, int rent)
    {
        var company = companyRepository.GetCompanyById(companyId) ?? throw new NullReferenceException(nameof(RPC_SyncRentPaid)); ;
        var owner = playerRepository.GetPlayerById(ownerId) ?? throw new NullReferenceException(nameof(RPC_SyncRentPaid)); ;
        var renter = playerRepository.GetPlayerById(playerId) ?? throw new NullReferenceException(nameof(RPC_SyncRentPaid)); ;

        owner.Money += rent;
        renter.Money -= rent;

        eventBus.Publish(new RentPaidEvent(companyId, playerId, company.OwnerId, rent));
    }

    #endregion RPC

}
