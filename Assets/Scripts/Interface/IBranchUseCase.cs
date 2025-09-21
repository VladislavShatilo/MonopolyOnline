using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IBranchUseCase 
{
    int BuyBranch(int companyId, int playerId);
    int SellBranch(int companyId, int playerId);
    void UpdateBranchUI(int companyId, int playerId, int newRentLevel);


}
