using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CompanyOfferInteractor
{
    private readonly ITradeService tradeService;
    private readonly ICompanyRepository companyRepository;

    public CompanyOfferInteractor(ITradeService tradeService, ICompanyRepository companyRepository)
    {
        this.tradeService = tradeService ?? throw new ArgumentNullException(nameof(tradeService));
        this.companyRepository = companyRepository ?? throw new ArgumentNullException(nameof(companyRepository));
    }

    public bool TryToggleCompanyInOffer(int companyId)
    {
        var company = companyRepository.GetCompanyById(companyId) ?? throw new InvalidOperationException(nameof(companyRepository));     

        if (!tradeService.IsTradeActive)
        {
            return false;
        }
        if (tradeService.CurrentOffer.FromPlayerData.Id == company.OwnerId || tradeService.CurrentOffer.ToPlayerData.Id == company.OwnerId)
        {
            bool isAlreadyInOffer = (tradeService.CurrentOffer.FromCompanies.Contains(company) ||
                                    tradeService.CurrentOffer.ToCompanies.Contains(company));
            if (isAlreadyInOffer)
            {
                tradeService.RemoveCompanyFromOffer(company.OwnerId, company);
            }
            else
            {
                tradeService.AddCompanyToOffer(company.OwnerId, company);
            }

            return true;
        }
        else
        {
            return false;
        }
    }
}