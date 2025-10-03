using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CompanyOfferInteractor
{
    private readonly ITradeService tradeService;
    private readonly ICompanyRepository companyRepository;

    public CompanyOfferInteractor(ITradeService tradeService, ICompanyRepository companyRepository)
    {
        this.tradeService = tradeService;
        this.companyRepository = companyRepository;
    }

    public bool TryToggleCompanyInOffer(int companyId)
    {
        var company = companyRepository.GetCompanyById(companyId);
        if (company == null)
        {
            return false;
        }

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