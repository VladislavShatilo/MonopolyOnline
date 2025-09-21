using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IBranchService 
{
    bool TryBuyBranch(int companyId, int playerId, out Company company);
    bool TrySellBranch(int companyId, int playerId, out Company company);
}
