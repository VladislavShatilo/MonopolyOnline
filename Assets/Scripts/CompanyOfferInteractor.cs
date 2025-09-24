using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CompanyOfferInteractor
{
    private readonly ITradeService tradeService;
    private readonly ICompanyRepository companyRepository;

    //[]
    //public CompanyOfferInteractor(ITradeService tradeService, ICompanyRepository companyRepository)
    //{
    //    this.tradeService = tradeService;
    //    this.companyRepository = companyRepository;
    //}

    //public bool TryToggleCompanyInOffer(int companyId)
    //{
    //    var company = companyRepository.GetCompanyById(companyId);
    //    if (company == null) return false;

    //    if (!tradeService.IsTradeActive || tradeService.CurrentPlayerId != company.OwnerId)
    //        return false;

    //    if (tradeService.IsCompanyInOffer(company))
    //    {
    //        tradeService.RemoveCompanyFromOffer(company.OwnerId, company);
    //    }
    //    else
    //    {
    //        tradeService.AddCompanyToOffer(company.OwnerId, company);
    //    }

    //    return true;
    //}
}
