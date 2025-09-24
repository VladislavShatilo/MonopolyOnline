using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPhotonMortgageManager 
{
    void RequestMortgageCompany(int companyId);
    void RequestBuyoutCompany(int companyId);
}
