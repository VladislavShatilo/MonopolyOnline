using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPhotonBranchManager
{
    void RequestBuyBranch(int companyId);
    void RequestSellBranch(int companyId);
}
