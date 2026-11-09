using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class PhotonMortgageSync : MonoBehaviourPun
{
    private IEventBus eventBus;
    private ICompanyRepository companyRepository;
    private ICompanyUIService companyUIService;
    private IPhotonViewWrapper photonViewWrapper;

    #region LIFE_CYCLE

    [Inject]
    public void Construct(IEventBus eventBus, ICompanyRepository companyRepository, ICompanyUIService companyUIService, IPhotonViewWrapper photonViewWrapper)
    {
        this.eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        this.companyRepository = companyRepository ?? throw new ArgumentNullException(nameof(companyRepository));
        this.companyUIService = companyUIService ?? throw new ArgumentNullException(nameof(companyUIService));
        this.photonViewWrapper = photonViewWrapper ?? throw new ArgumentNullException(nameof(photonViewWrapper));
        if (photonView == null) throw new NullReferenceException(nameof(photonView));
    }

    private void OnEnable()
    {
        eventBus.Subscribe<CompanyMortgagedEvent>(OnCompanyMortgaged);
        eventBus.Subscribe<CompanyBoughtBackEvent>(OnCompanyBoughtBack);
        eventBus.Subscribe<CompanyFreedFromMortgageEvent>(OnCompanyFreedFromMortgage);
        eventBus.Subscribe<CompanyTickUIEvent>(OnCompanyTickUI);
    }

    private void OnDisable()
    {
        eventBus.Unsubscribe<CompanyMortgagedEvent>(OnCompanyMortgaged);
        eventBus.Unsubscribe<CompanyBoughtBackEvent>(OnCompanyBoughtBack);
        eventBus.Unsubscribe<CompanyFreedFromMortgageEvent>(OnCompanyFreedFromMortgage);
        eventBus.Unsubscribe<CompanyTickUIEvent>(OnCompanyTickUI);
    }

    #endregion LIFE_CYCLE

    #region RPC

    [PunRPC]
    private void RPC_SetMortgage(int companyId, bool isMortgaged, int turns)
    {
        var company = companyRepository.GetCompanyById(companyId) ?? throw new NullReferenceException(nameof(RPC_SetMortgage));
        company.IsMortgaged = isMortgaged;
        company.MortgageTurnsLeft = turns;
    }

    [PunRPC]
    private void RPC_CompanyFreed(int companyId, bool isMortgaged, int turns)
    {
        var company = companyRepository.GetCompanyById(companyId) ?? throw new NullReferenceException(nameof(RPC_CompanyFreed));

        company.IsMortgaged = false;
        company.IsBought = false;
        company.OwnerId = -1;

        var ui = companyUIService.GetCompanyUI(companyId) ?? throw new NullReferenceException(nameof(RPC_CompanyFreed));
        ui.LoseCompanyUI(company);
    }

    [PunRPC]
    private void RPC_CompanyTickUI(int companyId, int turnsLeft)
    {
        var ui = companyUIService.GetCompanyUI(companyId) ?? throw new NullReferenceException(nameof(RPC_CompanyTickUI));
        ui.SetMortgageTurnsText(turnsLeft);
    }

    #endregion RPC

    #region CALLBACKS

    private void OnCompanyMortgaged(CompanyMortgagedEvent e)
    {
        photonViewWrapper.RPC(photonView, nameof(RPC_SetMortgage), RpcTarget.All, e.CompanyId, true, e.Turns);
    }

    private void OnCompanyBoughtBack(CompanyBoughtBackEvent e)
    {
        photonViewWrapper.RPC(photonView, nameof(RPC_SetMortgage), RpcTarget.All, e.CompanyId, false, 0);
    }

    private void OnCompanyFreedFromMortgage(CompanyFreedFromMortgageEvent e)
    {
        photonViewWrapper.RPC(photonView, nameof(RPC_CompanyFreed), RpcTarget.All, e.CompanyId, false, 0);
    }

    private void OnCompanyTickUI(CompanyTickUIEvent e)
    {
        photonViewWrapper.RPC(photonView, nameof(RPC_CompanyTickUI), RpcTarget.All, e.CompanyId, e.TurnsLeft);
    }

    #endregion CALLBACKS
}