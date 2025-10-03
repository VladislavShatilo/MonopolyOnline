using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICompanyTradeUI 
{
    void SetCompanyTradeUI(Company company, int ownerId);
    void SetRemoveAction(System.Action onAuction);
    void DestroySelf();
}
