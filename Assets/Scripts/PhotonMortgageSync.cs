using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class PhotonMortgageSync : MonoBehaviourPun
{
    private IEventBus eventBus;
    private ICompanyRepository companyRepository;
    private ICompanyUIService companyUIService;

    [Inject]
    public void Construct(IEventBus eventBus, ICompanyRepository companyRepository, ICompanyUIService companyUIService)
    {
        this.eventBus = eventBus;
        this.companyRepository = companyRepository;
        this.companyUIService = companyUIService;
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

    private void OnCompanyMortgaged(CompanyMortgagedEvent e)
    {
        photonView.RPC(nameof(RPC_SetMortgage), RpcTarget.All, e.CompanyId, true, e.Turns);
    }

    private void OnCompanyBoughtBack(CompanyBoughtBackEvent e)
    {
        photonView.RPC(nameof(RPC_SetMortgage), RpcTarget.All, e.CompanyId, false,0);
    }
    private void OnCompanyFreedFromMortgage(CompanyFreedFromMortgageEvent e)
    {
        photonView.RPC(nameof(RPC_CompanyFreed), RpcTarget.All, e.CompanyId, false, 0);
    }
    [PunRPC]
    private void RPC_SetMortgage(int companyId, bool isMortgaged,int turns)
    {
        var company = companyRepository.GetCompanyById(companyId);
        if (company != null)
        {
            company.IsMortgaged = isMortgaged;
            company.MortgageTurnsLeft  = turns;
        }
    }
    [PunRPC]
    private void RPC_CompanyFreed(int companyId, bool isMortgaged, int turns)
    {
        var company = companyRepository.GetCompanyById(companyId);
        if (company != null)
        {
            company.IsMortgaged = false;
            company.IsBought = false;
            company.OwnerId = -1;

        }
        var ui = companyUIService.GetCompanyUI(companyId);
        ui.LoseCompanyUI(company);
    }
    private void OnCompanyTickUI(CompanyTickUIEvent e)
    {
        photonView.RPC(nameof(RPC_CompanyTickUI), RpcTarget.All,e.CompanyId,e.TurnsLeft);
    }
    [PunRPC]
    private void RPC_CompanyTickUI(int companyId,int turnsLeft)
    {
        var ui = companyUIService.GetCompanyUI(companyId);
        ui.SetMortgageTurnsText(turnsLeft);

    }
}
public class CompanyTickUIEvent
{
    public int CompanyId { get; }

    public int TurnsLeft { get; }
    public CompanyTickUIEvent(int companyId, int turnsLeft)
    {
        CompanyId = companyId;
        TurnsLeft = turnsLeft;
    }
}