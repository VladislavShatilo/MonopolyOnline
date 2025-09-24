using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IMortgageService
{
    void MortgageCompany(int companyId, int playerId);
    void BuyoutCompany(int companyId, int playerId);
    void TickTurn(int playerId);
}
