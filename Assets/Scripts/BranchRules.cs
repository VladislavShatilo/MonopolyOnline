using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BranchRules 
{
    public bool CanShowBranchButtons(Company company, int currentPlayerId, bool ownsWholeGroup)
    {
        return company.IsBought
               && company.OwnerId == currentPlayerId
               && ownsWholeGroup
               && !company.IsMortgaged;
    }
}
